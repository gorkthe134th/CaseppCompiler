namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class UnconditionalJumpInstruction(int line, int column) : JumpInstruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("jump", null, null, Target.ToString());
    }
}
