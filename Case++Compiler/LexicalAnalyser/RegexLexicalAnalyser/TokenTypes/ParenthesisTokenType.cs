using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal partial class ParenthesisTokenType : TokenType
    {
        [GeneratedRegex(@"^[()]")]
        public override partial Regex Regex { get; }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(Position position, string text) =>
            new ParenthesisToken(position, 
                text switch
                {
                    "(" => RegionMarkType.Start,
                    ")" => RegionMarkType.End,
                    _   => throw new LexicalAnalyserException(position, $"Invalid Parenthesis \"{text}\"")
                });
    }
}
