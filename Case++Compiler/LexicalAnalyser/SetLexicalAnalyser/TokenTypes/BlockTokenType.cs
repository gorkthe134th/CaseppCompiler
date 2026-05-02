using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class BlockTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return static c => c == '{' | c == '}';
            }
        }

        public override int Limit => 1;

        public override Token GenerateToken(Position position, string text) =>
            new BlockToken(position,
                text switch
                {
                    "{" => RegionMarkType.Start,
                    "}" => RegionMarkType.End,
                    _   => throw new LexicalAnalyserException(position, $"Invalid Block Mark \"{text}\"")
                });
    }
}
