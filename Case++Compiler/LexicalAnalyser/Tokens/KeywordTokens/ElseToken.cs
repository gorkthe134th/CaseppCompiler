namespace CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens
{
    public class ElseToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Else";
    }
}
