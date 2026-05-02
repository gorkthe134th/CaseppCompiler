namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class SquareBracketToken(Position Position, RegionMarkType Type) : Token(Position)
    {
        public override string ToString() => $"Square Bracket {Type}";
    }
}
