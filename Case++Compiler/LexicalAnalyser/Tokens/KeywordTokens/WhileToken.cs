namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class WhileToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} While";
    }
}
