namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class RepeatToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Repeat";
    }
}
