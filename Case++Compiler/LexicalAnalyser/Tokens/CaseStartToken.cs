namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class CaseStartToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Case Start";
    }
}
