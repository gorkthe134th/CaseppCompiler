namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class EOFToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} EOF";
    }
}
