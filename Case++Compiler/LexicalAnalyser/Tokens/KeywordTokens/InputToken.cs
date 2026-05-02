namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class InputToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Input";
    }
}
