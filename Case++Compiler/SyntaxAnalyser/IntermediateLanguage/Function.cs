using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    // TODO: Replace error code with exceptions
    internal class Function : Symbol
    {
        public Function? Parent { get; }

        public string FullName => Parent != null ? Parent.FullName + "_" + Name : Name;

        internal bool IsMain { get; init; } = false;

        private readonly IList<FormalParameter> formalParameters = [];

        private readonly IList<Instruction> instructions = [];

        private Scope currentScope;
        private readonly HashSet<Variable> variablesInitialised = [];
        private readonly HashSet<Variable> variablesUsed = [];

        internal Variable? ReturnVariable { get; }

        private Dictionary<JumpInstruction, uint> breakOrigins = [];
        private readonly Stack<int> repeatTargets = [];
        private readonly BlockingCollection<Scope> scopes;

        public Function(string name, BlockingCollection<Scope> scopes, Function? parent = null) : base(name)
        {
            this.Parent = parent;
            this.ReturnVariable = new("_RET", true);
            this.currentScope = new(this, 0, parent?.currentScope, ReturnVariable) { IsBase = true }; // Cannot use field initializer for this
            this.scopes = scopes;
        }

        internal IReadOnlyList<FormalParameter> FormalParameters => formalParameters.AsReadOnly();

        public IReadOnlyList<Instruction> Instructions => instructions.AsReadOnly();

        public int CurrentInstructionIndex => instructions.Count;

        public int QuadCount => instructions.Count + 2;

        internal void AddParameter(FormalParameter formalParameter)
        {
            formalParameters.Add(formalParameter);
            if (!TryAddSymbol(formalParameter.AssociatedVariable))
                throw new InvalidOperationException($"Parameter \"{formalParameter.AssociatedVariable.Name}\" already exists.");
        }

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
            repeatTargets.Push(CurrentInstructionIndex);
            foreach (var kvp in breakOrigins) breakOrigins[kvp.Key]++;
        }

        internal void SetBreakPoint()
        {
            repeatTargets.Pop();

            var lookup = breakOrigins
                .Select(kvp => new KeyValuePair<JumpInstruction, uint>(kvp.Key, kvp.Value - 1))
                .ToLookup(kvp => kvp.Value > 0);

            breakOrigins = lookup[true].ToDictionary();
           lookup[false].Select(kvp => kvp.Key).Targets = CurrentInstructionIndex;
        }

        internal void SetAllBreakTargets()
        {
            breakOrigins.Keys.Targets = CurrentInstructionIndex;
            breakOrigins.Clear();
        }

        internal void EnterScope() => currentScope = new Scope(this, CurrentInstructionIndex, currentScope);

        internal void ExitScope()
        {
            currentScope.Exit(CurrentInstructionIndex);
            scopes.Add(currentScope);
            currentScope = currentScope.Parent ?? throw new InvalidOperationException("Cannot exit progenitor scope.");
        }
        // Could check parent nullability first to skip detaching the variables in case it is null,
        // but it looks cleaner this way and the progenitor scope never has variables, anyway.

        internal bool TryAddSymbol(Symbol symbol) => currentScope.TryAddSymbol(symbol);

        internal bool TryGetSymbol(string name, [NotNullWhen(true)] out Symbol? symbol) => currentScope.TryGetSymbol(name, out symbol);

        internal void InitialiseVariable(Variable variable)
        {
            if (!variablesUsed.Contains(variable)) variablesInitialised.Add(variable);
        }

        internal bool TryUseVariable(Variable variable)
        {
            if (variable.DeclaratingScope == currentScope && !variablesInitialised.Contains(variable)) return false;
            variablesUsed.Add(variable);
            return true;
        }

        internal bool MergeVariableDependancies(Function other, [NotNullWhen(false)] out IEnumerable<Variable>? uninitialisedVariables)
        {
            variablesInitialised.UnionWith(from variable in other.variablesInitialised
                                           where !variablesUsed.Contains(variable)
                                           select variable);
            var usedVariableGroups = from variable in other.variablesUsed
                                     where !variablesInitialised.Contains(variable)
                                     group variable by variable.DeclaratingScope == currentScope;
            uninitialisedVariables = null;
            IEnumerable<Variable>? initialisedVariables = null;
            foreach (var group in usedVariableGroups)
            {
                if (group.Key) { uninitialisedVariables = group; return false; }
                else initialisedVariables = group;
            }
            if (initialisedVariables != null) variablesUsed.UnionWith(initialisedVariables);
            return true;
        }

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
