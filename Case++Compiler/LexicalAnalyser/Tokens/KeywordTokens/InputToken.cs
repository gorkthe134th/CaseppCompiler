namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class InputToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Input";
    }
}
