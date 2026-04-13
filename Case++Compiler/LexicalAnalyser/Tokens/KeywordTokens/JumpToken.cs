namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class JumpToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Jump";
    }
}
