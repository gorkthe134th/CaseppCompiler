namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class InputInstruction(int line, int column, string variableID) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("in", variableID, null, null);
    }
}
