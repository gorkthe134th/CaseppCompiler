namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Variable(string name, bool isReference) : Symbol(name)
    {
        public bool IsReference { get; } = isReference;
    }
}
