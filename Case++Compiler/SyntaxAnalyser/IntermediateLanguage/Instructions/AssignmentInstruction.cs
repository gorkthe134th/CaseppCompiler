namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class AssignmentInstruction(int line, int column, string varID, object value) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => (":=", value.ToString(), null, varID);
    }
}
