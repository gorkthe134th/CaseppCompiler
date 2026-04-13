namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class RetvToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Retv";
    }
}
