namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class InToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} In";
    }
}
