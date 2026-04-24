namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols
{
    internal record class VariableSymbol(string Name, bool Reference) : Symbol(Name)
    {
        private bool initialised = false;
        internal bool IsInitialised { get => initialised; init => initialised = value; }

        internal void Initialise() => initialised = true;
    }
}
