namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class RepeatToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Repeat";
    }
}
