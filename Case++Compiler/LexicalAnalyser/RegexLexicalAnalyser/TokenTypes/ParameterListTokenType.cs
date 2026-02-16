using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class ParameterListTokenType : TokenType
    {
        [GeneratedRegex(@"^[()]")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(string text, int line, int column) =>
            new ParameterListToken(
                text switch
                {
                    "(" => RegionMarkType.Start,
                    ")" => RegionMarkType.End,
                    _   => throw new ArgumentException($"Line {line} Column {column}: Invalid Parameter Mark \"{text}\"")
                },
                line, column);
    }
}
