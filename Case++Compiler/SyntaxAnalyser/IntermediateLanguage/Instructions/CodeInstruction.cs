namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class CodeInstruction(Position Position, ReadOnlyMemory<char> Code) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("code", null, null, null);
    }
}
