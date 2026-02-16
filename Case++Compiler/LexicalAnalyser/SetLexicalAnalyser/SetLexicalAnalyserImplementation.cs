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
            new ParameterListTokenType(),
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
            int line = 1;
            int column = 0;

            // TODO: Do one final iteration with every predicate failing, to catch the last token
            while (!reader.EndOfStream || overflow.Count > 0)
            {
                if (!overflow.TryDequeue(out char character)) character = (char)reader.Read();
                if (character == '\n') { line++; column = -1; }
                column++;

                bool lastMatchingTypeUpdated = false;
                foreach ((TokenType type, IEnumerator<Func<char, bool?>> predicates) in tokenTypePredicates)
                {
                    if (!predicates.MoveNext())
                    {
                        lastMatchingType = type;
                        lastMatchingTypeUpdated = true;
                        addedOverflow.Clear();
                        addedOverflow.Enqueue(character);
                        tokenTypePredicates.Remove(type);
                        continue;
                    }
                    switch (predicates.Current(character))
                    {
                        case false:
                            tokenTypePredicates.Remove(type);
                            break;
                        case null:
                            lastMatchingType = type;
                            lastMatchingTypeUpdated = true;
                            addedOverflow.Clear();
                            break;
                        case true:
                            break;
                    }
                }

                if (!lastMatchingTypeUpdated) addedOverflow.Enqueue(character);

                currentText.Append(character);

                if (tokenTypePredicates.Count == 0)
                {
                    if (lastMatchingType == null)
                    {
                        currentText.Append(character);
                        throw new ArgumentException($"Line {line} Column {column}: Invalid Token \"{currentText}\"");
                    }

                    int startOffset = currentText.Length - 1;
                    string text = currentText.Remove(currentText.Length - addedOverflow.Count, addedOverflow.Count).ToString();
                    Token? token = lastMatchingType.GenerateToken(text, line, column - startOffset);
                    if (token != null) yield return token;

                    lastMatchingType = null;
                    currentText.Clear();

                    column -= addedOverflow.Count;
                    while (addedOverflow.TryDequeue(out char c)) overflow.Enqueue(c);
                    do
                    {
                        if (!overflow.TryDequeue(out character))
                        {
                            if (reader.EndOfStream) break;
                            character = (char)reader.Read();
                        }
                        if (character == '\n') { line++; column = -1; }
                        column++;
                    } while (char.IsWhiteSpace(character));
                    overflow.Enqueue(character);
                    column--;

                    tokenTypePredicates = GetTypePredicates();
                }
            }

            yield return new EOFToken(line, column);
        }

        private static Dictionary<TokenType, IEnumerator<Func<char, bool?>>> GetTypePredicates()
        {
            return tokenTypes.Select(t =>
                new KeyValuePair<TokenType, IEnumerator<Func<char, bool?>>>(t, t.CharacterPredicates.GetEnumerator()))
                .ToDictionary();
        }
    }
}
