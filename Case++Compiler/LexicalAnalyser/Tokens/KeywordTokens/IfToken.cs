namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class IfToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"If";
    }
}
