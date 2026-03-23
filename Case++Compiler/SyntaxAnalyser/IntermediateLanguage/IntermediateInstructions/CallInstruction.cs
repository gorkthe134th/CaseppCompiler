namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal class CallInstruction(int line, int column, string function) : Instruction(line, column)
    {
        public override string ToString() => $"call, {function}, _, _";
    }
}
