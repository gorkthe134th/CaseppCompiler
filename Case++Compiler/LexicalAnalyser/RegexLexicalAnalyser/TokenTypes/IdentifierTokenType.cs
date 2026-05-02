using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class IdentifierTokenType(KeywordTokenType keywordTokenType) : TokenType
    {
        [GeneratedRegex("^[A-Za-z0-9]{1,30}")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => IsLetterOrDigit;

        private static bool IsLetterOrDigit(char c) => 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9';

        public override Token GenerateToken(Position position, string text) =>
            keywordTokenType.Regex.Match(text).Length == text.Length
                ? keywordTokenType.GenerateToken(position, text)
                : new IdentifierToken(position, text);
    }
}
