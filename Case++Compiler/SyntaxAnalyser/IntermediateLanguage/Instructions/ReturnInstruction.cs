namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class ReturnInstruction(Position Position, Value? Value) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("retv", Value?.ToString(), null, null);
    }
}
