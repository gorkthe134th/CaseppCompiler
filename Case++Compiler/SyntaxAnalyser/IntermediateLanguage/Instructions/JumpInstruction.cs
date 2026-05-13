namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal abstract record class JumpInstruction(Position Position) : Instruction(Position)
    {
        protected TaskCompletionSource complete = new();

        public override Task Complete => complete.Task;

        public JumpInstruction(Position Position, CancellationToken? CancellationToken) : this(Position)
        {
            CancellationToken?.Register(() => complete.TrySetCanceled());
        }

        public int? Target
        {
            get => field;
            set
            {
                field = value;
                complete.TrySetResult();
            }
        } = null;
    }

    internal static class JumpInstructionExtensions
    {
        extension(IEnumerable<JumpInstruction> jumps)
        {
            public int? Targets { set { foreach (var jump in jumps) jump.Target = value; } }
        }
    }
}
