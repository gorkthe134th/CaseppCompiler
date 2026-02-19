namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class BlockToken(RegionMarkType type, int line, int column) : Token(line, column)
    {
        public RegionMarkType Type { get; } = type;

        public override string ToString() => $"{base.ToString()} Block {Type}";
    }
}
