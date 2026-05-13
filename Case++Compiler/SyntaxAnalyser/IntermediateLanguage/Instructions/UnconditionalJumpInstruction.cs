namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class UnconditionalJumpInstruction(Position Position, CancellationToken? CancellationToken = null)
        : JumpInstruction(Position, CancellationToken)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("jump", null, null, Target?.ToString());
    }
}
