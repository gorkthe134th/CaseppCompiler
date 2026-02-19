namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class IdentifierToken(string name, int line, int column) : Token(line, column)
    {
        public string Name { get; } = name;

        public override string ToString() => $"{base.ToString()} Identifier \"{Name}\"";
    }
}
