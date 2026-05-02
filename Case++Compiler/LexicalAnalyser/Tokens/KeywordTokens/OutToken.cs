namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class OutToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Out";
    }
}
