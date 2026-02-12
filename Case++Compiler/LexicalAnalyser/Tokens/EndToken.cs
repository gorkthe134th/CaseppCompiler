namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class EndToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} End";
    }
}
