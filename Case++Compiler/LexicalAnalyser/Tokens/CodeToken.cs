namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class CodeToken(Position Position, ReadOnlyMemory<char> Code) : Token(Position)
    {
        public override string ToString() => $"Code";
    }
}
