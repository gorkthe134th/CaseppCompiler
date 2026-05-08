using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    /// <summary>
    /// Represents a list of <see cref="Instruction"/>s.
    /// </summary>
    internal class Function : Symbol
    {
        /// <summary>
        /// The <see cref="Function"/> in which this <see cref="Function"/> was defined, if any.
        /// </summary>
        public Function? Parent { get; }

        /// <summary>
        /// A name that uniquely identifies the <see cref="Function"/>.
        /// </summary>
        public string FullName => Parent != null ? Parent.FullName + "_" + Name : Name;

        /// <summary>
        /// Indicates whether the <see cref="Function"/> is the first to be called when the final code is executed.
        /// </summary>
        /// <remarks>
        /// There must be only one <see cref="Function"/> marked as "Main" in a given program.
        /// </remarks>
        internal bool IsMain { get; init; } = false;

        private readonly IList<FormalParameter> formalParameters = [];

        public Stream<Instruction> Instructions { get; }

        private Scope? currentScope;
        private readonly HashSet<Variable> variablesInitialised = [];
        private readonly HashSet<Variable> variablesUsed = [];

        private Scope CurrentScope => currentScope ?? throw new InvalidOperationException("No available scopes.");

        /// <summary>
        /// The <see cref="Variable"/> containing the result of the <see cref="Function"/> execution, if any.
        /// </summary>
        internal Variable? ReturnVariable { get; }

        private Dictionary<JumpInstruction, uint> breakOrigins = [];
        private readonly Stack<int> repeatTargets = [];
        private readonly Stream<Scope> scopes;

        /// <param name="name">The name that the <see cref="Function"/> will be identified by.</param>
        /// <param name="scopes">The collection that will receive the <see cref="Scope"/>s created by the <see cref="Function"/>.</param>
        /// <param name="parent">The <see cref="Function"/> in which this <see cref="Function"/> was defined, if any.</param>
        public Function(string name, Stream<Scope> scopes, Function? parent = null, int? instructionCapacity = null, CancellationToken? cancellationToken = null) : base(name)
        {
            this.Parent = parent;
            this.ReturnVariable = new("_RET", true);
            this.currentScope = new(this, 0, parent?.currentScope, ReturnVariable) { IsBase = true }; // Cannot use field initializer for this
            scopes.AddAsync(CurrentScope).Wait();
            this.scopes = scopes;
            this.Instructions = new(instructionCapacity, cancellationToken);
        }

        /// <summary>
        /// The <see cref="FormalParameter"/>s of the <see cref="Function"/>.
        /// </summary>
        internal IReadOnlyList<FormalParameter> FormalParameters => formalParameters.AsReadOnly();

        /// <summary>
        /// The Instruction Index of the next <see cref="Instruction"/> to be added.
        /// </summary>
        internal int CurrentInstructionIndex { get; private set; }

        /// <summary>
        /// The number of Quads that whould be produced if the <see cref="ToQuads(int)"/> method was called on this <see cref="Function"/>.
        /// </summary>
        internal int QuadCount => CurrentInstructionIndex + 2;

        /// <summary>
        /// Adds a <see cref="FormalParameter"/> to the <see cref="Function"/>.
        /// </summary>
        /// <param name="formalParameter">The <see cref="FormalParameter"/> to add.</param>
        /// <exception cref="ArgumentException">Invalid Symbol for the current Scope.</exception>
        internal void AddParameter(FormalParameter formalParameter)
        {
            formalParameters.Add(formalParameter);
            AddSymbol(formalParameter.AssociatedVariable);
        }

        internal void AddReturnValueParameter()
        {
            if (ReturnVariable != null)
                formalParameters.Add(new TypeRestrictedFormalParameter<OutParameter>(ReturnVariable));
        }

        internal Task AddInstruction(Instruction instruction)
        {
            CurrentInstructionIndex++;
            return Instructions.AddAsync(instruction);
        }

        internal async Task AddBreak(JumpInstruction jump, uint count)
        {
            await AddInstruction(jump);
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

        internal Task EnterScope()
        {
            currentScope = new Scope(this, CurrentInstructionIndex, currentScope);
            return scopes.AddAsync(CurrentScope);
        }

        internal void ExitScope()
        {
            Scope scope = CurrentScope;
            try
            {
                scope.Exit(CurrentInstructionIndex);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"Cannot exit the current Scope.", e);
            }
            currentScope = scope.Parent;
            if (scope.IsBase) Instructions.Complete();
        }

        /// <summary>
        /// Adds a symbol to the current scope.
        /// </summary>
        /// <param name="symbol">The symbol to add.</param>
        /// <exception cref="ArgumentException">Invalid Symbol for the current Scope.</exception>
        internal void AddSymbol(Symbol symbol)
        {
            try
            {
                CurrentScope.AddSymbol(symbol);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Invalid Symbol \"{symbol.Name}\" for the current Scope.", e);
            }
            symbol.DeclaratingFunction = this;
        }

        /// <summary>
        /// Gets the <see cref="Symbol"/> in the current <see cref="Scope"/> with the specified name, if it exists; otherwise, calls <paramref name="symbolFactory"/> and adds the result to the current <see cref="Scope"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Symbol"/> to search.</param>
        /// <param name="symbolFactory">The method to call if the requested <see cref="Symbol"/> is not found.</param>
        /// <returns>The <see cref="Symbol"/> in the current <see cref="Scope"/> with the specified name or the result of calling <paramref name="symbolFactory"/>.</returns>
        internal Symbol GetOrAddSymbol(string name, Func<Symbol> symbolFactory) => CurrentScope.GetOrAddSymbol(name, symbolFactory);

        /// <summary>
        /// Gets the <see cref="Symbol"/> in the current <see cref="Scope"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the <see cref="Symbol"/> to search.</param>
        /// <returns>The <see cref="Symbol"/> with the specified name defined in either this <see cref="Function"/> or any of its ancestors.</returns>
        /// <exception cref="ArgumentException">Inaccessible Symbol from the current Scope.<see cref="Scope"/>.</exception>
        internal Symbol GetSymbol(string name)
        {
            try
            {
                return CurrentScope[name];
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Inaccessible Symbol \"{name}\" from the current Scope.", e);
            }
        }

        /// <summary>
        /// Marks the specified <see cref="Variable"/> as initialised by the <see cref="Function"/>.
        /// </summary>
        /// <param name="variable">The <see cref="Variable"/> to mark as initialised.</param>
        internal void InitialiseVariable(Variable variable)
        {
            if (!variablesUsed.Contains(variable)) variablesInitialised.Add(variable);
        }

        /// <summary>
        /// Marks the specified <see cref="Variable"/> as used by the <see cref="Function"/>.
        /// </summary>
        /// <param name="variable">The <see cref="Variable"/> to mark as used.</param>
        /// <exception cref="InvalidOperationException">Use of uninitialised variable.</exception>
        internal void UseVariable(Variable variable)
        {
            if (variable.DeclaratingFunction == this && !variablesInitialised.Contains(variable))
                throw new InvalidOperationException($"Use of uninitialised variable {variable.Name}.");
            variablesUsed.Add(variable);
        }

        /// <summary>
        /// Marks <see cref="Variable"/>s as initialised or used by this <see cref="Function"/>, based on the initialisation or usage of them by <paramref name="other"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <exception cref="InvalidOperationException">Use of uninitialised variables in function.</exception>
        internal void MergeVariableDependancies(Function other)
        {
            variablesInitialised.UnionWith(from variable in other.variablesInitialised
                                           where !variablesUsed.Contains(variable)
                                           select variable);
            var usedVariableGroups = from variable in other.variablesUsed
                                     where !variablesInitialised.Contains(variable)
                                     group variable by variable.DeclaratingFunction == this;
            foreach (var group in usedVariableGroups)
            {
                if (group.Key == false) variablesUsed.UnionWith(group);
                else throw new InvalidOperationException($"Use of uninitialised variables in function \"{other.Name}\": {string.Join(", ", group.Select(v => $"\"{v.Name}\""))}.");
            }
        }

        public async IAsyncEnumerable<(string?, string?, string?, string?)> ToQuads(int offset)
        {
            yield return ("begin_block", FullName, null, null);
            await foreach (var instruction in Instructions.GetAsyncEnumerable(i => i.Complete))
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
