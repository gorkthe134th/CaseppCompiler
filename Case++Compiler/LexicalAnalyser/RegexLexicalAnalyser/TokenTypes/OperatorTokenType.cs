using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class OperatorTokenType : TokenType
    {
        [GeneratedRegex(@"^(\+|-|\*|/|(<>)|(<=)|(>=)|=|<|>|(not(?![A-Za-z0-9]))|(and(?![A-Za-z0-9]))|(or(?![A-Za-z0-9])))")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(Position position, string text) =>
            new OperatorToken(position, OperationType.FromSymbol(text));
    }
}
