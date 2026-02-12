using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.TokenTypes
{
    internal partial class ConstantTokenType : TokenType
    {
        [GeneratedRegex(@"^[0-9]+")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(string text, int line, int column) =>
            new ConstantToken(uint.Parse(text), line, column);
    }
}
