using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Scope(Scope? parent = null)
    {
        internal Scope? Parent { get; } = parent;

        private readonly Dictionary<string, Symbol> symbols = [];

        internal bool TryAddSymbol(Symbol symbol) => symbols.TryAdd(symbol.Name, symbol);

        internal bool TryGetSymbol(string name, [NotNullWhen(true)] out Symbol? symbol) =>
            symbols.TryGetValue(name, out symbol) || (Parent?.TryGetSymbol(name, out symbol) ?? false);
    }
}
