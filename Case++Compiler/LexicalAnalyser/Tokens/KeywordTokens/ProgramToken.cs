namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class ProgramToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Program";
    }
}
