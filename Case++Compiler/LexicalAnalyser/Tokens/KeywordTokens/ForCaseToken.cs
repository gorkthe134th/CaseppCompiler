namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class ForCaseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} ForCase";
    }
}
