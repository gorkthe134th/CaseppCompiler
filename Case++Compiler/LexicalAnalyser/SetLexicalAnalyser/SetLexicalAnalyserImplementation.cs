using CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes;
using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Collections.Concurrent;
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

        public void Analyse(Stream input, BlockingCollection<Token>? output = null)
        {
            StreamReader reader = new(input);
            Queue<char> overflow = new();

            Dictionary<TokenType, IEnumerator<Func<char, bool?>>> tokenTypePredicates;
            StringBuilder currentText = new();
            Queue<char> addedOverflow = new();
            int line = 1, column = 0;
            int matchStartLine = 1, matchStartColumn = 1;
            int matchEndLine = -1, matchEndColumn = -1;

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
                    if (character == '\n') { line++; column = -1; }
                    column++;
                } while (char.IsWhiteSpace(character));

                matchStartLine = line;
                matchStartColumn = column;

                overflow.Enqueue(character);
                column--;

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
                        if (character == '\n') { line++; column = -1; }
                        column++;
                    }

                    bool updatedLongestMatchingType = false;
                    int? limit = null;
                    foreach ((TokenType type, IEnumerator<Func<char, bool?>> predicates) in tokenTypePredicates)
                    {
                        if (!predicates.MoveNext())
                        {
                            longestMatchingType = type;
                            updatedLongestMatchingType = true;
                            matchEndLine = line;
                            matchEndColumn = column - 1;
                            addedOverflow.Clear();
                            if (!eof)
                            {
                                addedOverflow.Enqueue(character);
                                if (character == '\n')
                                {
                                    matchEndLine--;
                                    matchEndColumn = -1;
                                }
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
                                matchEndLine = line;
                                matchEndColumn = column;
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
                    throw new LexicalAnalyserException($"Line {line} Column {column}: Invalid Token \"{currentText}\"");

                currentText.Remove(currentText.Length - addedOverflow.Count, addedOverflow.Count);
                Token? token = longestMatchingType.GenerateToken(currentText.ToString(), matchStartLine, matchStartColumn);
                if (token != null) output?.Add(token);

                currentText.Clear();

                line = matchEndLine;
                column = matchEndColumn;

                while (addedOverflow.TryDequeue(out character)) overflow.Enqueue(character);
            }
            output?.Add(new EOFToken(line, column + 1));
            output?.CompleteAdding();
        }
    }
}
