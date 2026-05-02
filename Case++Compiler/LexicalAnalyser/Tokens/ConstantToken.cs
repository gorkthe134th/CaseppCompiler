namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public record class ConstantToken(Position Position, uint Constant) : Token(Position)
    {
        public uint Constant { get; } = Constant <= 32767 ? Constant :
            throw new LexicalAnalyserException(Position, $"Constants must be in range [-32767, 32767].");

        public override string ToString() => $"Constant {Constant}";
    }
}
