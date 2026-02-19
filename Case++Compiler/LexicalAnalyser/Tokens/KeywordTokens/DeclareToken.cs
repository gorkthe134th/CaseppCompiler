namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class DeclareToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Declare";
    }
}
