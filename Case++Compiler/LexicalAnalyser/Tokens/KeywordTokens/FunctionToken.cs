namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class FunctionToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Function";
    }
}
