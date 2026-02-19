namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class PrintToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Print";
    }
}
