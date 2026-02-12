namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class OutToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Out";
    }
}
