namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class ParenthesisToken(Position Position, RegionMarkType Type) : Token(Position)
    {
        public override string ToString() => $"Parenthesis {Type}";
    }
}
