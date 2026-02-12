namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class WhileCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} WhileCase";
    }
}
