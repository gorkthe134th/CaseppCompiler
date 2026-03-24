namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class CallInstruction(int line, int column, string function) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("call", function, null, null);
    }
}
