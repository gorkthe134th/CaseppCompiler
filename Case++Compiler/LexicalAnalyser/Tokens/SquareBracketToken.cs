namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class SquareBracketToken(RegionMarkType type, int line, int column) : Token(line, column)
    {
        public RegionMarkType Type { get; } = type;

        public override string ToString() => $"{base.ToString()} Square Bracket {Type}";
    }
}
