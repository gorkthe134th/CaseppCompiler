namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class IdentifierToken(Position Position, string Name) : Token(Position)
    {
        public override string ToString() => $"Identifier \"{Name}\"";
    }
}
