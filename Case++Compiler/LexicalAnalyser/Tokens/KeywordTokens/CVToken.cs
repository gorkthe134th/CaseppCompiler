namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class CVToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"CV";
    }
}
