namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class RetvToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Retv";
    }
}
