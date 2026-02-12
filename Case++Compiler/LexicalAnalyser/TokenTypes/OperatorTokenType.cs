using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.TokenTypes
{
    internal partial class OperatorTokenType : TokenType
    {
        [GeneratedRegex(@"^((not)|\+|-|\*|/|=|<|>|(<>)|(<=)|(>=)|(and)|(or))")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(string text, int line, int column) =>
            new OperatorToken(text, line, column);
    }
}
