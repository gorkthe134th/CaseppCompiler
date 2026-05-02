using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Scope(Function encompassingFunction, int start, Scope? parent = null, params IEnumerable<Symbol> initialSymbols)
    {
        internal int Start { get; } = start;

        internal int? End { get; private set; }

        internal bool IsBase { get; init; } = false;

        internal Function EncompassingFunction { get; } = encompassingFunction;

        internal Scope? Parent { get; } = parent;

        private readonly Dictionary<string, Symbol> symbols = initialSymbols.Select(s => new KeyValuePair<string, Symbol>(s.Name, s)).ToDictionary();

        internal ICollection<Symbol> Symbols => symbols.Values;

        internal bool TryAddSymbol(Symbol symbol)
        {
            if (!symbols.TryAdd(symbol.Name, symbol)) return false;
            symbol.DeclaratingScope = this;
            return true;
        }

        internal bool TryGetSymbol(string name, [NotNullWhen(true)] out Symbol? symbol) =>
            symbols.TryGetValue(name, out symbol) || (Parent?.TryGetSymbol(name, out symbol) ?? false);

        internal void Exit(int end)
        {
            End = end;
            foreach ((string _, Symbol symbol) in symbols) symbol.ForgetScope();
        }
    }
}
