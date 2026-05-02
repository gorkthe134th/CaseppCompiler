namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class ReturnToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Return";
    }
}
