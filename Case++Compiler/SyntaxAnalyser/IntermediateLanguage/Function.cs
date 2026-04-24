using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Function(string name, Function? parent = null)
    {
        public Function? Parent { get; } = parent;

        public string Name { get; } = name;

        public string FullName => Parent != null ? Parent.FullName + "_" + Name : Name;

        private readonly IList<Instruction> instructions = [];
        private Scope currentScope = new(parent?.currentScope);
        private Dictionary<JumpInstruction, uint> breakOrigins = [];
        private readonly Stack<int> repeatTargets = [];

        public IReadOnlyList<Instruction> Instructions => instructions.AsReadOnly();

        public int CurrentPosition => instructions.Count;

        public int QuadCount => instructions.Count + 2;

        internal void AddInstruction(Instruction instruction) => instructions.Add(instruction);

        internal void AddBreak(JumpInstruction jump, uint count)
        {
            instructions.Add(jump);
            breakOrigins.Add(jump, count);
        }

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
                .Select(kvp => new KeyValuePair<JumpInstruction, uint>(kvp.Key, kvp.Value - 1))
                .ToLookup(kvp => kvp.Value > 0);

            breakOrigins = lookup[true].ToDictionary();
           lookup[false].Select(kvp => kvp.Key).Targets = CurrentPosition;
        }

        internal void SetAllBreakTargets()
        {
            breakOrigins.Keys.Targets = CurrentPosition;
            breakOrigins.Clear();
        }

        internal void EnterScope() => currentScope = new Scope(parent: currentScope);

        internal void ExitScope() => currentScope = currentScope.Parent ?? throw new InvalidOperationException("Cannot exit head function scope.");

        internal bool TryAddSymbol(Symbol symbol) => currentScope.TryAddSymbol(symbol);

        internal bool TryGetSymbol(string name, [NotNullWhen(true)] out Symbol? symbol) => currentScope.TryGetSymbol(name, out symbol);

        public IEnumerable<(string?, string?, string?, string?)> ToQuads(int offset)
        {
            yield return ("begin_block", FullName, null, null);
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
            yield return ("end_block", FullName, null, null);
        }
    }
}
