namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class InCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} InCase";
    }
}
