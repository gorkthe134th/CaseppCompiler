using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class CodeTokenType : TokenType
    {
        [GeneratedRegex(@"^\${.*\$}", RegexOptions.Singleline)]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(Position position, string text) =>
            new CodeToken(position, text.AsMemory().Slice(2, text.Length - 4));
    }
}
