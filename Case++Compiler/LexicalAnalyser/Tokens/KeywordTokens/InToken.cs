namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class InToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"In";
    }
}
