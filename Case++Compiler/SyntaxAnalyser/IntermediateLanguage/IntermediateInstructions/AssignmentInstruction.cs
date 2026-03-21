namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal class AssignmentInstruction(int line, int column, string varID, object resultID) : Instruction(line, column)
    {
        public override string ToString() => $":=, {varID}, {resultID}, _";
    }
}
