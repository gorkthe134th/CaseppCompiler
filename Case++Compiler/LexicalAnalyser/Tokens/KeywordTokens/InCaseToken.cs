namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class InCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} InCase";
    }
}
