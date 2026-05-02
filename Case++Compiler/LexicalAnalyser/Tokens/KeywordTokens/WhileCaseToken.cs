namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class WhileCaseToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"WhileCase";
    }
}
