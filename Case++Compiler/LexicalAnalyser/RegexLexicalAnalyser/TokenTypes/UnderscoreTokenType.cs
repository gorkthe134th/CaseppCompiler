using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class UnderscoreTokenType : TokenType
    {
        [GeneratedRegex(@"^_")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(Position position, string text) =>
            new UnderscoreToken(position);
    }
}
