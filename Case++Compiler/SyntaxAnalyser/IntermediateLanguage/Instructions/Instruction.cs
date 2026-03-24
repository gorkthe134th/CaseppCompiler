namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal abstract class Instruction(int line, int column)
    {
        public int Line { get; } = line;

        public int Column { get; } = column;

        public abstract (string?, string?, string?, string?) ToQuad();
    }
}
