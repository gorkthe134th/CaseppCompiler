namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class CallToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Call";
    }
}
