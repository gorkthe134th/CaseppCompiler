namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class OutputInstruction(Position Position, Value Value) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("out", Value.ToString(), null, null);
    }
}
