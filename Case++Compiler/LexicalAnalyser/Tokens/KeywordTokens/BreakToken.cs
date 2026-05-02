namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class BreakToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Break";
    }
}
