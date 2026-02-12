using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.TokenTypes
{
    internal partial class ParameterTokenType : TokenType
    {
        [GeneratedRegex(@"^[()]")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(string text, int line, int column) =>
            new ParameterToken(
                text switch
                {
                    "(" => RegionMarkType.Start,
                    ")" => RegionMarkType.End,
                    _   => throw new ArgumentException($"Line {line} Column {column}: Invalid Parameter Mark \"{text}\"")
                },
                line, column);
    }
}
