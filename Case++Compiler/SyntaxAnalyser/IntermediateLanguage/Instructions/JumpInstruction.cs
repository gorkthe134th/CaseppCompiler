namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal abstract record class JumpInstruction(Position Position) : Instruction(Position)
    {
        public int? Target { get; set; } = null;
    }

    internal static class JumpInstructionExtensions
    {
        extension(IEnumerable<JumpInstruction> jumps)
        {
            public int? Targets { set { foreach (var jump in jumps) jump.Target = value; } }
        }
    }
}
