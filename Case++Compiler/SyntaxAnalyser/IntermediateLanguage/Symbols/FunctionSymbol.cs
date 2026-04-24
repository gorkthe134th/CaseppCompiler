namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols
{
    internal record class FunctionSymbol(Function Function) : Symbol(Function.Name)
    {
        private readonly IList<IFormalParameter> formalParameters = [];

        internal IReadOnlyList<IFormalParameter> FormalParameters => formalParameters.AsReadOnly();

        internal void AddParameter(IFormalParameter formalParameter) => formalParameters.Add(formalParameter);
    }
}
