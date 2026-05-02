namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class HashToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Hash";
    }
}
