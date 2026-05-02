namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public abstract record class Token(Position Position)
    {
        public override string ToString() => "$Invalid Token$";
    }
}
