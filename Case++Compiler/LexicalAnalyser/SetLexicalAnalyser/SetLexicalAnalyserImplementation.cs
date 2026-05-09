using CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes;
using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser
{
    internal class SetLexicalAnalyserImplementation : ILexicalAnalyser
    {
        private readonly static ICollection<TokenType> tokenTypes =
        [
            new AssignmentTokenType(),
            new BlockTokenType(),
            new ColonTokenType(),
            new CommaTokenType(),
            new ConstantTokenType(),
            new IdentifierTokenType(),
            new OperatorTokenType(),
            new ParenthesisTokenType(),
            new SquareBracketTokenType(),
            new SemiColonTokenType(),
            new HashTokenType(),
            new UnderscoreTokenType(),
            new SingleLineCommentTokenType(),
            new MultiLineCommentTokenType(),
        ];

        public async Task Analyse(Stream input, Stream<Token>? output = null, CancellationToken? cancellationToken = null)
        {
            try
            {
                StreamReader reader = new(input);
                Queue<char> overflow = new();

                Dictionary<TokenType, IEnumerator<Func<char, bool?>>> tokenTypePredicates;
                StringBuilder currentText = new();
                Queue<char> addedOverflow = new();
                Position currentPosition = new(1, 0);
                Position matchStart = new(1, 1);
                Position matchEnd = new(-1, -1);

                bool IsOnlyWhiteSpace()
                {
                    char character;
                    do
                    {
                        if (!overflow.TryDequeue(out character))
                        {
                            if (reader.EndOfStream) return true;
                            character = (char)reader.Read();
                        }
                        if (character == '\n') currentPosition.GoToNextLine();
                        else currentPosition++;
                    } while (char.IsWhiteSpace(character));

                    matchStart = currentPosition;

                    overflow.Enqueue(character);
                    currentPosition--;

                    return false;
                }

                while (!IsOnlyWhiteSpace())
                {
                    char character;
                    bool eof;

                    tokenTypePredicates = tokenTypes.Select(t =>
                        new KeyValuePair<TokenType, IEnumerator<Func<char, bool?>>>(t, t.CharacterPredicates.GetEnumerator()))
                        .ToDictionary();
                    TokenType? longestMatchingType = null;

                    do
                    {
                        eof = true;
                        if (overflow.TryDequeue(out character)) eof = false;
                        else
                        {
                            int readResult = reader.Read();
                            if (readResult != -1)
                            {
                                character = (char)readResult;
                                eof = false;
                            }
                        }
                        if (!eof)
                        {
                            if (character == '\n') currentPosition.GoToNextLine();
                            else currentPosition++;
                        }

                        bool updatedLongestMatchingType = false;
                        int? limit = null;
                        foreach ((TokenType type, IEnumerator<Func<char, bool?>> predicates) in tokenTypePredicates)
                        {
                            if (!predicates.MoveNext())
                            {
                                longestMatchingType = type;
                                updatedLongestMatchingType = true;
                                matchEnd = currentPosition - 1;
                                addedOverflow.Clear();
                                if (!eof)
                                {
                                    addedOverflow.Enqueue(character);
                                    if (character == '\n') matchEnd.GoToPreviousLine();
                                }
                                tokenTypePredicates.Remove(type);
                                continue;
                            }
                            switch (eof ? false : predicates.Current(character))
                            {
                                case false:
                                    tokenTypePredicates.Remove(type);
                                    break;
                                case null:
                                    longestMatchingType = type;
                                    updatedLongestMatchingType = true;
                                    if (limit == null || type.Limit > limit) limit = type.Limit;
                                    matchEnd = currentPosition;
                                    addedOverflow.Clear();
                                    break;
                                case true:
                                    if (limit == null || type.Limit > limit) limit = type.Limit;
                                    break;
                            }
                        }

                        if (!eof)
                        {
                            if (!updatedLongestMatchingType) addedOverflow.Enqueue(character);
                            currentText.Append(character);
                            if (limit != null && currentText.Length > (int)limit)
                                currentText.Remove((int)limit, currentText.Length - (int)limit);
                        }
                    } while (tokenTypePredicates.Count > 0);

                    if (longestMatchingType == null)
                        throw new LexicalAnalyserException(currentPosition, $"Invalid Token \"{currentText}\"");

                    currentText.Remove(currentText.Length - addedOverflow.Count, addedOverflow.Count);
                    Token? token = longestMatchingType.GenerateToken(matchStart, currentText.ToString());
                    if (token != null && output != null) await output.AddAsync(token);

                    currentText.Clear();

                    currentPosition = matchEnd;

                    while (addedOverflow.TryDequeue(out character)) overflow.Enqueue(character);
                }
                if (output != null) await output.AddAsync(new EOFToken(currentPosition + 1));
            }
            finally
            {
                output?.Complete();
            }
        }
    }
}
