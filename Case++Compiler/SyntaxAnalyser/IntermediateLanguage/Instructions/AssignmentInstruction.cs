namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class AssignmentInstruction(int line, int column, string varID, object result) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => (":=", varID, result.ToString(), null);
    }
}
