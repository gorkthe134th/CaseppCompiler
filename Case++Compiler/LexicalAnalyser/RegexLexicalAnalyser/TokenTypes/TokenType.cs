using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal abstract class TokenType
    {
        public abstract Regex Regex { get; }

        public abstract Predicate<char>? Trim { get; }

        public abstract Token GenerateToken(Position position, string text);
    }
}
