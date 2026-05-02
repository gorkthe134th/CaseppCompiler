namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class ForCaseToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"ForCase";
    }
}
