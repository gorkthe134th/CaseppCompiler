namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal abstract class Symbol(string name)
    {
        public string Name { get; } = name;

        private Scope? declaratingScope;
        internal Scope DeclaratingScope
        {
            get => declaratingScope ?? throw new InvalidOperationException($"Variable \"{Name}\" is free. The Declarating Scope is unknown.");
            set => declaratingScope = value;
        }

        internal virtual void ForgetScope() => declaratingScope = null;
    }
}
