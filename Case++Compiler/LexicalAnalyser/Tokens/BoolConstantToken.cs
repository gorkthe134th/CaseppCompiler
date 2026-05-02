namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class BoolConstantToken(Position Position, bool Constant) : Token(Position)
    {
        public override string ToString() => $"Constant {Constant}";
    }
}
