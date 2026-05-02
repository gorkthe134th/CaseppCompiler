using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class ConstantTokenType : TokenType
    {
        [GeneratedRegex(@"^[0-9]+")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(Position position, string text) =>
            new ConstantToken(position, uint.Parse(text));
    }
}
