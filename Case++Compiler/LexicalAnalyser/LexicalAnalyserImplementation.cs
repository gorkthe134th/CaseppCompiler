using CaseppCompiler.LexicalAnalyser.TokenTypes;
using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser
{
    internal class LexicalAnalyserImplementation
    {
        private static readonly TokenType[] tokenTypes;

        static LexicalAnalyserImplementation()
        {
            ConstantTokenType constantTokenType = new();
            OperatorTokenType operatorTokenType = new();
            IdentifierTokenType identifierTokenType = new(operatorTokenType);

            tokenTypes =
            [
                constantTokenType,
                identifierTokenType,
                operatorTokenType,
            ];
        }

        public IEnumerable<Token> Analyse(Stream input)
        {
            InputStream inputStream = new(input);
            int line = 1;
            int column = 1;
            while (!inputStream.EndOfStream)
            {
                inputStream.Trim(ref line, ref column);
                if (inputStream.EndOfStream) yield break;
                TokenType? matchedType = null;
                if (inputStream.TryMatchFirst(tokenTypes.Select(type => (matchedType = type).Regex()), out string text) && matchedType != null)
                {
                    yield return matchedType.GenerateToken(text, line, column);
                    column += text.Length;
                }
                else
                {
                    throw new ArgumentException($"Line {line} Column {column}: Invalid Token");
                }
            }
        }
    }
}
