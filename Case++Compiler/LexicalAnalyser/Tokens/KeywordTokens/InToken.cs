namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class InToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} In";
    }
}
