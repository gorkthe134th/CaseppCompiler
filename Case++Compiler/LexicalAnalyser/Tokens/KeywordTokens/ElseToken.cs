namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public record class ElseToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Else";
    }
}
