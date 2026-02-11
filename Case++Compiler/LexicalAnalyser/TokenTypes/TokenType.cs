using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.TokenTypes
{
    internal abstract class TokenType
    {
        public abstract Regex Regex();

        public abstract Token GenerateToken(string text, int line, int column);
    }
}
