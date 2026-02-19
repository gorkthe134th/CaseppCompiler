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
            new CaseStartTokenType(),
            new CommaTokenType(),
            new ConstantTokenType(),
            new IdentifierTokenType(),
            new OperatorTokenType(),
            new ParenthesisTokenType(),
            new SquareBracketTokenType(),
            new SemiColonTokenType(),
            new SingleLineCommentTokenType(),
            new MultiLineCommentTokenType(),
        ];

        public IEnumerable<Token> Analyse(Stream input)
        {
            StreamReader reader = new(input);
            Queue<char> overflow = new();

            Dictionary<TokenType, IEnumerator<Func<char, bool?>>> tokenTypePredicates = GetTypePredicates();
            StringBuilder currentText = new();
            TokenType? lastMatchingType = null;
            Queue<char> addedOverflow = new();
            int line = 1, column = 0;
            int matchStartLine = 1, matchStartColumn = 1;
            int matchEndLine = -1, matchEndColumn = -1;

            while (!reader.EndOfStream || overflow.Count > 0 || addedOverflow.Count > 0)
            {
                char character;
                bool eof = true;

                if (overflow.TryDequeue(out character)) eof = false;
                else if (!reader.EndOfStream) { character = (char)reader.Read(); eof = false; }
                if (!eof)
                {
                    if (character == '\n') { line++; column = -1; }
                    column++;
                }

                bool updatedLastMatchingType = false;
                int? limit = null;
                foreach ((TokenType type, IEnumerator<Func<char, bool?>> predicates) in tokenTypePredicates)
                {
                    if (!predicates.MoveNext())
                    {
                        lastMatchingType = type;
                        updatedLastMatchingType = true;
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
                            lastMatchingType = type;
                            updatedLastMatchingType = true;
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
                    if (!updatedLastMatchingType) addedOverflow.Enqueue(character);
                    currentText.Append(character);
                    if (limit != null && currentText.Length > (int)limit)
                        currentText.Remove((int)limit, currentText.Length - (int)limit);
                }

                if (tokenTypePredicates.Count == 0)
                {
                    if (lastMatchingType == null)
                    {
                        if (!eof) currentText.Append(character);
                        throw new ArgumentException($"Line {line} Column {column}: Invalid Token \"{currentText}\"");
                    }

                    currentText.Remove(currentText.Length - addedOverflow.Count, addedOverflow.Count);
                    Token? token = lastMatchingType.GenerateToken(currentText.ToString(), matchStartLine, matchStartColumn);
                    if (token != null) yield return token;

                    lastMatchingType = null;
                    currentText.Clear();

                    line = matchEndLine;
                    column = matchEndColumn;

                    while (addedOverflow.TryDequeue(out character)) overflow.Enqueue(character);
                    do
                    {
                        if (!overflow.TryDequeue(out character))
                        {
                            if (reader.EndOfStream) goto exit;
                            character = (char)reader.Read();
                        }
                        if (character == '\n') { line++; column = -1; }
                        column++;
                    } while (char.IsWhiteSpace(character));

                    matchStartLine = line;
                    matchStartColumn = column;

                    overflow.Enqueue(character);
                    column--;

                    tokenTypePredicates = GetTypePredicates();
                }
            }
        exit:
            yield return new EOFToken(line, column + 1);
        }

        private static Dictionary<TokenType, IEnumerator<Func<char, bool?>>> GetTypePredicates()
        {
            return tokenTypes.Select(t =>
                new KeyValuePair<TokenType, IEnumerator<Func<char, bool?>>>(t, t.CharacterPredicates.GetEnumerator()))
                .ToDictionary();
        }
    }
}
