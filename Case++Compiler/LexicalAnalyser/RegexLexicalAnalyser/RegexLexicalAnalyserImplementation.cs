using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser
{
    internal class RegexLexicalAnalyserImplementation : ILexicalAnalyser
    {
        private static readonly TokenType[] tokenTypes;

        static RegexLexicalAnalyserImplementation()
        {
            ConstantTokenType constantTokenType = new();
            OperatorTokenType operatorTokenType = new();
            KeywordTokenType keywordTokenType = new();
            IdentifierTokenType identifierTokenType = new(operatorTokenType, keywordTokenType);
            AssignmentTokenType assignmentTokenType = new();
            CaseStartTokenType caseStartTokenType = new();
            BlockTokenType blockTokenType = new();
            ParameterListTokenType parameterTokenType = new();
            CommaTokenType commaTokenType = new();
            SemiColonTokenType endTokenType = new();

            tokenTypes =
            [
                constantTokenType,
                identifierTokenType,
                operatorTokenType,
                assignmentTokenType,
                caseStartTokenType,
                blockTokenType,
                parameterTokenType,
                commaTokenType,
                endTokenType,
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
                if (inputStream.TryMatchFirst(tokenTypes.Select(type => (matchedType = type).Regex), out string text) && matchedType != null)
                {
                    yield return matchedType.GenerateToken(text, line, column);
                    column += text.Length;
                    Predicate<char>? trim = matchedType.Trim;
                    if (trim != null) inputStream.Trim(trim, ref line, ref column);
                }
                else
                {
                    throw new ArgumentException($"Line {line} Column {column}: Invalid Token");
                }
            }
        }
    }
}
