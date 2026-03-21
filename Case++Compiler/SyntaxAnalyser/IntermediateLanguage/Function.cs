using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Function
    {
        public string Name { get; set; } = "$Invalid Name$";
        private readonly IList<Instruction> instructions = [];

        public int CurrentPosition => instructions.Count;

        public void AddInstruction(Instruction instruction) => instructions.Add(instruction);

        public void SetJumpTargets(IList<int> positions, int target)
        {
            foreach (int p in positions)
                if (instructions[p] is JumpInstruction jump) jump.Target = target;
                else throw new InvalidOperationException($"Instruction {p} is not a jump instruction.");
        }

        public void WriteToFile(StreamWriter writer, ref int offset)
        {
            int start = offset;
            writer.WriteLine($"begin_block, {Name}, _, _");
            foreach (var instruction in instructions)
            {
                if (instruction is JumpInstruction jump) jump.Target = start + jump.Target;
                writer.WriteLine($"{offset++}: {instruction}");
            }
            writer.WriteLine($"end_block, {Name}, _, _");
        }
    }
}
