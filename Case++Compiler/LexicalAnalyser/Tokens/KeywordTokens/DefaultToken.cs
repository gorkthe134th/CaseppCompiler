namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class DefaultToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Default";
    }
}
