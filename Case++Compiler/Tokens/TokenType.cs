using System.Text.RegularExpressions;

namespace CaseppCompiler.Tokens
{
    internal abstract class TokenType
    {
        public abstract Regex Regex();

        public abstract Token GenerateToken(string text, int line);
    }
}
