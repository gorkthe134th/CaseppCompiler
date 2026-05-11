namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    /// <summary>
    /// Provides a collection of <see cref="Symbol"/>s for a scecified region of code.
    /// </summary>
    /// <param name="encompassingFunction">The <see cref="Function"/> for which the <see cref="Scope"/> applies.</param>
    /// <param name="start">The first Instruction Index for which this <see cref="Scope"/> applies.</param>
    /// <param name="parent">The <see cref="Scope"/> the must be applied before this <see cref="Scope"/> can be applied, if any.</param>
    /// <param name="initialSymbols">The <see cref="Symbol"/>s initially provided by the <see cref="Scope"/>. More <see cref="Symbol"/> may be added later.</param>
    internal class Scope(Function encompassingFunction, int start, Scope? parent = null, params IEnumerable<Symbol> initialSymbols)
    {
        /// <summary>
        /// The first Instruction Index for which this <see cref="Scope"/> applies.
        /// </summary>
        internal int Start { get; } = start;

        internal delegate void EndedHandler(Scope sender, int end);
        internal event EndedHandler? Ended;

        /// <summary>
        /// Indicates whether the <see cref="Scope"/> is the Base <see cref="Scope"/> of its Encompassing <see cref="Function"/>.
        /// </summary>
        /// <remarks>
        /// When a <see cref="Function"/> is called, its Base <see cref="Scope"/> must be the first <see cref="Scope"/> to be loaded, since it contains the <see cref="Function"/> Parameter <see cref="Symbol"/>s.
        /// </remarks>
        internal bool IsBase { get; init; } = false;

        /// <summary>
        /// The <see cref="Function"/> for which the <see cref="Scope"/> applies.
        /// </summary>
        internal Function EncompassingFunction { get; } = encompassingFunction;

        /// <summary>
        /// The <see cref="Scope"/> the must be applied before this <see cref="Scope"/> can be applied, if any.
        /// </summary>
        internal Scope? Parent { get; } = parent;

        public Lock SymbolLock { get; } = new();
        private readonly Dictionary<string, Symbol> symbols = initialSymbols.Select(s => new KeyValuePair<string, Symbol>(s.Name, s)).ToDictionary();

        /// <summary>
        /// The <see cref="Symbol"/>s provided by the <see cref="Scope"/>.
        /// </summary>
        internal ICollection<Symbol> Symbols => symbols.Values;

        /// <summary>
        /// Gets the <see cref="Symbol"/> in the <see cref="Scope"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the <see cref="Symbol"/> to search.</param>
        /// <returns>The <see cref="Symbol"/> with the specified name provided by either this <see cref="Scope"/> or any of its ancestors.</returns>
        /// <exception cref="ArgumentException">Cannot find Symbol through this Scope.</exception>
        internal Symbol this[string name]
        {
            get
            {
                lock (SymbolLock)
                {
                    return symbols.TryGetValue(name, out Symbol? symbol) ? symbol : Parent?[name] ??
                        throw new ArgumentException($"Cannot find Symbol \"{name}\" through this Scope.");
                }
            }
        }

        internal delegate void SymbolAddedHandler(Scope sender, Symbol symbol);
        internal event SymbolAddedHandler? SymbolAdded;

        /// <summary>
        /// Gets the <see cref="Symbol"/> in the <see cref="Scope"/> with the specified name, if it exists; otherwise, calls <paramref name="symbolFactory"/> and adds the result to the <see cref="Scope"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Symbol"/> to search.</param>
        /// <param name="symbolFactory">The method to call if the requested <see cref="Symbol"/> is not found.</param>
        /// <returns>The <see cref="Symbol"/> in the <see cref="Scope"/> with the specified name or the result of calling <paramref name="symbolFactory"/>.</returns>
        internal Symbol GetOrAddSymbol(string name, Func<Symbol> symbolFactory)
        {
            lock (SymbolLock)
            {
                if (!symbols.TryGetValue(name, out Symbol? symbol))
                {
                    symbol = symbolFactory.Invoke();

                    symbols.Add(symbol.Name, symbol);

                    SymbolAdded?.Invoke(this, symbol);
                }
                return symbol;
            }
        }

        /// <summary>
        /// Adds a <see cref="Symbol"/> to the <see cref="Scope"/>.
        /// </summary>
        /// <param name="symbol">The <see cref="Symbol"/> to add.</param>
        /// <exception cref="ArgumentException">Symbol already exists.</exception>
        internal void AddSymbol(Symbol symbol)
        {
            lock (SymbolLock)
            {
                try
                {
                    symbols.Add(symbol.Name, symbol);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Symbol \"{symbol.Name}\" already exists.", e);
                }
                SymbolAdded?.Invoke(this, symbol);
            }
        }

        /// <summary>
        /// Marks the end of the <see cref="Scope"/>.
        /// </summary>
        /// <param name="end">The Instruction Index from which this <see cref="Scope"/> will no longer apply.</param>
        /// <remarks>
        /// This methods also calls the <see cref="Symbol.ForgetFunction"/> method on all the provided <see cref="Symbol"/>s, allowing the <see cref="Scope"/> to be Disposed by the Garbage Collector.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Cannot exit Scope due to Symbol.</exception>
        internal void Exit(int end)
        {
            lock (SymbolLock)
            {
                Symbol? lastSymbol = null;
                try
                {
                    foreach ((string _, Symbol symbol) in symbols)
                    {
                        lastSymbol = symbol;
                        symbol.ForgetFunction();
                    }
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidOperationException($"Cannot exit Scope due to Symbol \"{lastSymbol?.Name}\".", e);
                }
                Ended?.Invoke(this, end);
            }
        }
    }
}
