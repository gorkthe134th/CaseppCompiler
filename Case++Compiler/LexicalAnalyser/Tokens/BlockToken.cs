namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class BlockToken(Position Position, RegionMarkType Type) : Token(Position)
    {
        public override string ToString() => $"Block {Type}";
    }
}
