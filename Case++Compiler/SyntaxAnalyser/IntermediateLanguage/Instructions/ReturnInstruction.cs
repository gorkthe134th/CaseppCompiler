namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class ReturnInstruction(int line, int column, object value) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("retv", value.ToString(), null, null);
    }
}
