namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class CaseStartToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Case Start";
    }
}
