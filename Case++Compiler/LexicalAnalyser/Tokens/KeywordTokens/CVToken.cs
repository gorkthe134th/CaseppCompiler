namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class CVToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} CV";
    }
}
