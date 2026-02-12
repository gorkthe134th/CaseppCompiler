namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class IfToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} If";
    }
}
