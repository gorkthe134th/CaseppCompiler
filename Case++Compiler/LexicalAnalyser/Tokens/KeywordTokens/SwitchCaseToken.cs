namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class SwitchCaseToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"SwitchCase";
    }
}
