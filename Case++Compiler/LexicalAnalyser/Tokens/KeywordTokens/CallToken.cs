namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class CallToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Call";
    }
}
