using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.TokenTypes
{
    internal partial class EndTokenType : TokenType
    {
        [GeneratedRegex(@"^;")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(string text, int line, int column) =>
            new EndToken(line, column);
    }
}
