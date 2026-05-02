namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class OperatorToken(Position Position, OperationType Operation) : Token(Position)
    {
        public override string ToString() => $"{Operation}";
    }
}
