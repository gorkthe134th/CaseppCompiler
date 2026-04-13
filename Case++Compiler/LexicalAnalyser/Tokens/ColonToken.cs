namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class ColonToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Case Start";
    }
}
