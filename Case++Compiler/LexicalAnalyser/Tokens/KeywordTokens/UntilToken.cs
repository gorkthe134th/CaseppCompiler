namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class UntilToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Until";
    }
}
