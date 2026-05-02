namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class CommaToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Comma";
    }
}
