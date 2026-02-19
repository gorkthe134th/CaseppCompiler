namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class UntilCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} UntilCase";
    }
}
