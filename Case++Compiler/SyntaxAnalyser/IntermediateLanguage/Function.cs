using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Function(Function? parent = null)
    {
        private readonly IList<Instruction> instructions = [];
        private Dictionary<int, uint> breakOrigins = [];
        private readonly Stack<int> repeatTargets = [];

        public string Name { get => Parent != null ? Parent.Name + "_" + field : field; set => field = value; } = "$Invalid Name$";

        public Function? Parent { get; } = parent;

        public IReadOnlyList<Instruction> Instructions => instructions.AsReadOnly();

        public int CurrentPosition => instructions.Count;

        public int QuadCount => instructions.Count + 2;

        internal void AddInstruction(Instruction instruction) => instructions.Add(instruction);

        internal void SetJumpTargets(IEnumerable<int> positions, int target)
        {
            foreach (int p in positions)
                if (instructions[p] is JumpInstruction jump) jump.Target = target;
                else throw new InvalidOperationException($"Instruction {p} is not a jump instruction.");
        }

        internal void AddBreak(uint count) => breakOrigins.Add(CurrentPosition, count);

        internal int GetRepeatPoint(uint index)
        {
            int target = 0; // Default to 0 when repeatTargets is empty. Meaning, when there are no blocks or loops, jump to the start of the function.
            var e = repeatTargets.GetEnumerator();
            while (index-- > 0 && e.MoveNext()) target = e.Current;
            return target;
        }

        internal void SetRepeatPoint()
        {
            repeatTargets.Push(CurrentPosition);
            foreach (var kvp in breakOrigins) breakOrigins[kvp.Key]++;
        }

        internal void SetBreakPoint()
        {
            repeatTargets.Pop();

            var lookup = breakOrigins
                .Select(kvp => new KeyValuePair<int, uint>(kvp.Key, kvp.Value - 1))
                .ToLookup(kvp => kvp.Value > 0);

            breakOrigins = lookup[true].ToDictionary();
            SetJumpTargets(lookup[false].Select(kvp => kvp.Key), CurrentPosition);
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
                if (instruction is JumpInstruction jump)
                {
                    var localTarget = jump.Target;
                    jump.Target = localTarget + offset + 1;
                    (string?, string?, string?, string?) quad = jump.ToQuad();
                    jump.Target = localTarget;
                    yield return quad;
                    continue;
                }
                yield return instruction.ToQuad();
            }
            yield return ("end_block", Name, null, null);
        }
    }
}
