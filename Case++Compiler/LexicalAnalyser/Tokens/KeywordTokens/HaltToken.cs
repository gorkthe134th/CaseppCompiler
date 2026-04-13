namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class HaltToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Halt";
    }
}
