namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class UntilCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} UntilCase";
    }
}
