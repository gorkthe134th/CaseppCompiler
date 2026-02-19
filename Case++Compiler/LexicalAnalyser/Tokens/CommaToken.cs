namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class CommaToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Comma";
    }
}
