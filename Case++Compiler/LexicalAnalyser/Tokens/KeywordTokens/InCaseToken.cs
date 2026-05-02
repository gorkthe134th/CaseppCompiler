namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class InCaseToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"InCase";
    }
}
