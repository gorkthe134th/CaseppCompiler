namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class AssignmentToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} Assignment";
    }
}
