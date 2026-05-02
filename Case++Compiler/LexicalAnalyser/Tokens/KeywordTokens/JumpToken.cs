namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class JumpToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Jump";
    }
}
