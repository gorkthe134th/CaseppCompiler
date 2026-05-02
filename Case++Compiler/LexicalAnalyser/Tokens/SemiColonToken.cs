namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class SemiColonToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Semi Colon";
    }
}
