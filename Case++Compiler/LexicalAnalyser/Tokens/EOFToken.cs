namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class EOFToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"EOF";
    }
}
