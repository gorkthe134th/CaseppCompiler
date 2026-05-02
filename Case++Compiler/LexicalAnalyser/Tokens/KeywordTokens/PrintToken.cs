namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class PrintToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Print";
    }
}
