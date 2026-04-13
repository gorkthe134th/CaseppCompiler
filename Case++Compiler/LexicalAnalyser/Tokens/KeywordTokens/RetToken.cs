namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class RetToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Ret";
    }
}
