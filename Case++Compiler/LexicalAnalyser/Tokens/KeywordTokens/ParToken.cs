namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class ParToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Par";
    }
}
