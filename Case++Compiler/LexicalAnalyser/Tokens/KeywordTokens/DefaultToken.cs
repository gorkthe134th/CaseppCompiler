namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class DefaultToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Default";
    }
}
