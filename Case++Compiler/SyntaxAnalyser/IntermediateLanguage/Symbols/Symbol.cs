namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols
{
    internal abstract record class Symbol(string Name)
    {
        private Scope? declaratingScope;
        internal Scope DeclaratingScope
        {
            get => declaratingScope ?? throw new InvalidOperationException($"Variable \"{Name}\" is free. The Declarating Scope is unknown.");
            set => declaratingScope = value;
        }

        internal void ForgetScope() => declaratingScope = null;
    }
}
