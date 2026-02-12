namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class InOutToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} InOut";
    }
}
