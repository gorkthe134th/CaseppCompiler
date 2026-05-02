namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class UntilCaseToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"UntilCase";
    }
}
