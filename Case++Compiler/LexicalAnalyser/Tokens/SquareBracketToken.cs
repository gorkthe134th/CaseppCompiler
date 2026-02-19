namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class SquareBracketToken(RegionMarkType type, int line, int column) : Token(line, column)
    {
        public RegionMarkType Type { get; } = type;

        public override string ToString() => $"{base.ToString()} Square Bracket {Type}";
    }
}
