namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class UntilToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Until";
    }
}
