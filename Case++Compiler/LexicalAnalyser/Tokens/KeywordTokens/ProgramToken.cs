namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    internal class ProgramToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Program";
    }
}
