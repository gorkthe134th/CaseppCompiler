using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Function
    {
        public string Name { get; set; } = "$Invalid Name$";
        private readonly IList<Instruction> instructions = [];
        private Dictionary<int, uint> breakOrigins = [];

        public int CurrentPosition => instructions.Count;

        public void AddInstruction(Instruction instruction) => instructions.Add(instruction);

        public void SetJumpTargets(IEnumerable<int> positions, int target)
        {
            foreach (int p in positions)
                if (instructions[p] is JumpInstruction jump) jump.Target = target;
                else throw new InvalidOperationException($"Instruction {p} is not a jump instruction.");
        }

        internal void AddBreak(uint count)
        {
            if (count == 0) return;
            breakOrigins.Add(CurrentPosition, count);
        }

        internal void SetBreakTargets()
        {
            IList<int> breaksToSet = [];

            breakOrigins = breakOrigins.Where(kvp =>
            {
                if (kvp.Value > 1) return true;
                breaksToSet.Add(kvp.Key);
                return false;
            }).Select(kvp => new KeyValuePair<int, uint>(kvp.Key, kvp.Value - 1)).ToDictionary();

            SetJumpTargets(breaksToSet, CurrentPosition);
        }

        internal void SetAllBreakTargets()
        {
            SetJumpTargets(breakOrigins.Keys, CurrentPosition);
            breakOrigins.Clear();
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
