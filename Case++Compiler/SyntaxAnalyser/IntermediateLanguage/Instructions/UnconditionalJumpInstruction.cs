namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class UnconditionalJumpInstruction(int line, int column, int? target) : JumpInstruction(line, column, target)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("jump", null, null, Target.ToString());
    }
}
