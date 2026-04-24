namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class OutputInstruction(int line, int column, Value value) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("out", value.ToString(), null, null);
    }
}
