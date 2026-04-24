namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal abstract class JumpInstruction(int line, int column, int? target) : Instruction(line, column)
    {
        public int? Target { get; set; } = target;
    }

    internal static class JumpInstructionExtensions
    {
        extension(IEnumerable<JumpInstruction> jumps)
        {
            public int? Targets { set { foreach (var jump in jumps) jump.Target = value; } }
        }
    }
}
