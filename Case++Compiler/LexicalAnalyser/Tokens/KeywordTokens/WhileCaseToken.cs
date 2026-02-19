namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class WhileCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} WhileCase";
    }
}
