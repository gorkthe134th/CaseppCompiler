namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class FunctionToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Function";
    }
}
