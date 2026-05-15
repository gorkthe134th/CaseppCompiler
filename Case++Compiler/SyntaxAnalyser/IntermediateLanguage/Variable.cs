namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Variable(string name, bool isReference, bool requireInitialisation) : Symbol(name)
    {
        public bool IsReference { get; } = isReference;

        private bool requireInitialisation = requireInitialisation;

        public void Initialise()
        {
            requireInitialisation = false;
        }

        internal override void ForgetFunction()
        {
            if (requireInitialisation) throw new InvalidOperationException("This Variable needs to be assigned a value.");
            base.ForgetFunction();
        }
    }
}
