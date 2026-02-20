using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class OperatorTokenType : TokenType
    {
        [GeneratedRegex(@"^(\+|-|\*|/|(<>)|(<=)|(>=)|=|<|>|(not(?![A-Za-z]))|(and(?![A-Za-z]))|(or(?![A-Za-z])))")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(string text, int line, int column) =>
            new OperatorToken(text, line, column);
    }
}
