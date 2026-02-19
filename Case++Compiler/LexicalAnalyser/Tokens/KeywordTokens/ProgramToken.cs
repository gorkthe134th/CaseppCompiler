namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class ProgramToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Program";
    }
}
