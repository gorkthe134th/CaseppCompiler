namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class WhenToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"When";
    }
}
