namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class HaltInstruction(int line, int column) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("halt", null, null, null);
    }
}
