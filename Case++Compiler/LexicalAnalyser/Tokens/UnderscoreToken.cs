namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class UnderscoreToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Underscore";
    }
}
