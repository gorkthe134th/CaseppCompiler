using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal record class JumpBlockInfo(List<JumpInstruction> TrueOriginList, List<JumpInstruction> FalseOriginList, int Start);
    internal record class ExpressionBlockInfo(Value Result, int Start);

    internal record class FunctionCallBlockInfo(Function Function, int Start)
    {
        private readonly IList<ActualParameter> actualParameters = [];

        internal IReadOnlyList<ActualParameter> Parameters => actualParameters.AsReadOnly();

        internal void AddParameter(ActualParameter parameter) => actualParameters.Add(parameter);
    }
}
