namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class ConstantToken : Token
    {
        public ConstantToken(uint constant, int line, int column) : base(line, column)
        {
            Constant = constant < 32767 ? constant :
                throw new ArgumentException($"{base.ToString()} Constants must be in range [-32767, 32767]");
        }

        public uint Constant { get; }

        public override string ToString() => $"{base.ToString()} {Constant}";
    }
}
