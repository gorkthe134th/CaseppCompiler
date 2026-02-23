namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class BoolConstantToken(bool constant, int line, int column) : Token(line, column)
    {
        public bool Constant { get; } = constant;

        public override string ToString() => $"{base.ToString()} {Constant}";
    }
}
