namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public abstract class Token(int line, int column)
    {
        public int Line { get; } = line;

        public int Column { get; } = column;

        public override string ToString() => $"Line {Line} Column {Column}:";
    }
}
