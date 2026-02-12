namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class CommaToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Comma";
    }
}
