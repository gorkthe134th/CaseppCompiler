namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class DeclareToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Declare";
    }
}
