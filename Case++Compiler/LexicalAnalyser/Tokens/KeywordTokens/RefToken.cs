namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class RefToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Ref";
    }
}
