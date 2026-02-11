using System.Text.RegularExpressions;

namespace CaseppCompiler.Tokens
{
    internal partial class ConstantTokenType : TokenType
    {
        [GeneratedRegex(@"^[0-9]+")]
        public override partial Regex Regex();

        public override Token GenerateToken(string text, int line) => new Token(text, line);
    }
}
