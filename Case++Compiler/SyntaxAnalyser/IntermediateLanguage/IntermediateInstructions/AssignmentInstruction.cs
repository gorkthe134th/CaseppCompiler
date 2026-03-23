namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal class AssignmentInstruction(int line, int column, string varID, object result) : Instruction(line, column)
    {
        public override string ToString() => $":=, {varID}, {result}, _";
    }
}
