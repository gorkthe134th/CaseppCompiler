using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class SquareBracketTokenType : TokenType
    {
        [GeneratedRegex(@"^[\[\]]")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(string text, int line, int column) =>
            new ParenthesisToken(
                text switch
                {
                    "[" => RegionMarkType.Start,
                    "]" => RegionMarkType.End,
                    _   => throw new ArgumentException($"Line {line} Column {column}: Invalid Bracket Mark \"{text}\"")
                },
                line, column);
    }
}
