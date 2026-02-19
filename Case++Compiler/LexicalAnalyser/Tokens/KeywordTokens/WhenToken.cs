namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class WhenToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} When";
    }
}
