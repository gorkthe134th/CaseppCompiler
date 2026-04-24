using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal record class JumpBlockInfo(List<JumpInstruction> TrueOriginList, List<JumpInstruction> FalseOriginList, int Start);
    internal record class ExpressionBlockInfo(Value Result, int Start);

    internal record class FunctionCallBlockInfo(FunctionSymbol FunctionSymbol, int Start)
    {
        private readonly IList<IActualParameter> actualParameters = [];
        private readonly IEnumerator<IFormalParameter> formalParameters = FunctionSymbol.FormalParameters.GetEnumerator();

        internal IReadOnlyList<IActualParameter> Parameters => actualParameters.AsReadOnly();

        internal bool TryAddParameter(IActualParameter actualParameter, out string? errorMessage)
        {
            if (!formalParameters.MoveNext()) { errorMessage = null; return false; }
            if (!formalParameters.Current.IsMatch(actualParameter, out errorMessage)) return false;
            actualParameters.Add(actualParameter);
            return true;
        }
    }
}
