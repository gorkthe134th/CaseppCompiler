namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class SwitchCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} SwitchCase";
    }
}
