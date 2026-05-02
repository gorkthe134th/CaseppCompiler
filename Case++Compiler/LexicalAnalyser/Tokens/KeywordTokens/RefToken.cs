namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class RefToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Ref";
    }
}
