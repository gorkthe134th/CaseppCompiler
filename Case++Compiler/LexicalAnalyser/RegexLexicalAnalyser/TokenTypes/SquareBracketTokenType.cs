using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class SquareBracketTokenType : TokenType
    {
        [GeneratedRegex(@"^[\[\]]")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(Position position, string text) =>
            new SquareBracketToken(position, 
                text switch
                {
                    "[" => RegionMarkType.Start,
                    "]" => RegionMarkType.End,
                    _   => throw new LexicalAnalyserException(position, $"Invalid Bracket Mark \"{text}\"")
                });
    }
}
