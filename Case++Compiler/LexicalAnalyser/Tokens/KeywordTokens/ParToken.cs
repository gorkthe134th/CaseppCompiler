namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class ParToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Par";
    }
}
