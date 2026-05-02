namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class AssignmentToken(Position Position) : Token(Position)
    {
        public override string ToString() => $"Assignment Token";
    }
}
