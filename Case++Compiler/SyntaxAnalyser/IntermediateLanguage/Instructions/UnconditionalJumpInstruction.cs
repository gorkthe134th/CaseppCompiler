namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class UnconditionalJumpInstruction(Position Position) : JumpInstruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("jump", null, null, Target?.ToString());
    }
}
