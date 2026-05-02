namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class HaltToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Halt";
    }
}
