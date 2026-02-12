namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class AssignmentToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Assignment";
    }
}
