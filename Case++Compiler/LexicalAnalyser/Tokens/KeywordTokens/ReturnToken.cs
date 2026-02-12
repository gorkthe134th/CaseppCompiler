namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class ReturnToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Return";
    }
}
