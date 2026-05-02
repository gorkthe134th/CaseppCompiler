namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class RetToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Ret";
    }
}
