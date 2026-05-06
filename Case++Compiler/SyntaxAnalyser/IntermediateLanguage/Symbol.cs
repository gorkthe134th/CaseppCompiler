namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal abstract class Symbol(string name)
    {
        public string Name { get; } = name;

        private Function? declaratingFunction;
        internal Function DeclaratingFunction
        {
            get => declaratingFunction ??
                throw new InvalidOperationException($"Variable \"{Name}\" is free. The Declarating Function is unknown.");
            set => declaratingFunction = declaratingFunction == null ? value :
                throw new InvalidOperationException("The Declarating Function has already been set.");
        }

        internal virtual void ForgetScope() => declaratingFunction = null;
    }
}
