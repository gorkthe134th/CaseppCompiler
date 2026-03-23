namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class BreakToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Break";
    }
}
