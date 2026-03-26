using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Function
    {
        public string Name { get; set; } = "$Invalid Name$";
        private readonly IList<Instruction> instructions = [];
        private Dictionary<int, uint> breakOrigins = [];

        public int CurrentPosition => instructions.Count;

        public int Length => instructions.Count;

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

        internal void IncreaseAllBreaks(uint count)
        {
            foreach (var kvp in breakOrigins) breakOrigins[kvp.Key] = kvp.Value + count;
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

        public IEnumerable<(string?, string?, string?, string?)> ToQuads(int offset)
        {
            yield return ("begin_block", Name, null, null);
            foreach (var instruction in instructions)
            {
                if (instruction is JumpInstruction jump) jump.Target = offset + jump.Target;
                yield return instruction.ToQuad();
            }
            yield return ("end_block", Name, null, null);
        }
    }
}
