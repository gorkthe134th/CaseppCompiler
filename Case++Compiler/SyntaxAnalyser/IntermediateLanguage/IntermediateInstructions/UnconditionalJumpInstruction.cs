namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal class UnconditionalJumpInstruction(int line, int column) : JumpInstruction(line, column)
    {
        public override string ToString() => $"jump, _, _, {base.ToString()}";
    }
}
