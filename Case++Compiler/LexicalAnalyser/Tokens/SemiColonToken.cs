namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class SemiColonToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Semi Colon";
    }
}
