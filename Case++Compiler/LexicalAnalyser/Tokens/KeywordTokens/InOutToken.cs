namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class InOutToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"InOut";
    }
}
