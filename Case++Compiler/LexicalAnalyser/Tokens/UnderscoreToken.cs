namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class UnderscoreToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Underscore";
    }
}
