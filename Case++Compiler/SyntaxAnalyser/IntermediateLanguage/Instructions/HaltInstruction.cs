namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class HaltInstruction(Position Position) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("halt", null, null, null);
    }
}
