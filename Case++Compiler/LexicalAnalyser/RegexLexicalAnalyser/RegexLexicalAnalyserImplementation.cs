using CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes;
using CaseppCompiler.LexicalAnalyser.Tokens;

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
            IdentifierTokenType identifierTokenType = new(keywordTokenType);
            AssignmentTokenType assignmentTokenType = new();
            ColonTokenType caseStartTokenType = new();
            BlockTokenType blockTokenType = new();
            ParenthesisTokenType parameterTokenType = new();
            SquareBracketTokenType squareBracketTokenType = new();
            CommaTokenType commaTokenType = new();
            SemiColonTokenType endTokenType = new();
            HashTokenType hashTokenType = new();
            UnderscoreTokenType underscoreTokenType = new();

            tokenTypes =
            [
                constantTokenType,
                operatorTokenType,
                identifierTokenType,
                assignmentTokenType,
                caseStartTokenType,
                blockTokenType,
                parameterTokenType,
                squareBracketTokenType,
                commaTokenType,
                endTokenType,
                hashTokenType,
                underscoreTokenType,
            ];
        }

        public async Task Analyse(Stream input, Stream<Token>? output = null, CancellationToken? cancellationToken = null)
        {
            try
            {
                InputStream inputStream = new(input);
                Position position = new(1, 1);
                while (!inputStream.EndOfStream)
                {
                    inputStream.Trim(ref position);
                    if (inputStream.EndOfStream) break;
                    TokenType? matchedType = null;
                    if (inputStream.TryMatchFirst(tokenTypes.Select(type => (matchedType = type).Regex), out string text) && matchedType != null)
                    {
                        var token = matchedType.GenerateToken(position, text);
                        if (output != null) await output.AddAsync(token);
                        position += text.Length;
                        Predicate<char>? trim = matchedType.Trim;
                        if (trim != null) inputStream.Trim(trim, ref position);
                    }
                    else
                    {
                        throw new LexicalAnalyserException(position, $"Invalid Token");
                    }
                }
                if (output != null) await output.AddAsync(new EOFToken(position));
            }
            finally
            {
                output?.Complete();
            }
        }
    }
}
