namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class InOutToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} InOut";
    }
}
