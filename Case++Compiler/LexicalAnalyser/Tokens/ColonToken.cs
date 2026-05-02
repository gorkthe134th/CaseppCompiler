namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class ColonToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Colon";
    }
}
