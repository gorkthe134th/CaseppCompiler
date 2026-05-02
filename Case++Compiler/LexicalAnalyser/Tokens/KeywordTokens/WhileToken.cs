namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class WhileToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"While";
    }
}
