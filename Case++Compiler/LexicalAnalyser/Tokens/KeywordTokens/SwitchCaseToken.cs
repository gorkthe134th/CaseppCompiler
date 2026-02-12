namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class SwitchCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} SwitchCase";
    }
}
