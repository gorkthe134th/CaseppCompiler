using System.Collections.Immutable;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class ComparisonJumpInstruction(Position Position, OperationType Operation, Value Operand1, Value Operand2, CancellationToken? CancellationToken = null)
        : JumpInstruction(Position, CancellationToken)
    {
        private static readonly ImmutableDictionary<OperationType, string> operationMap = new Dictionary<OperationType, string>()
        {
            [OperationType.EqualTo             ] = "=",
            [OperationType.LessThan            ] = "<",
            [OperationType.GreaterThan         ] = ">",
            [OperationType.NotEqualTo          ] = "<>",
            [OperationType.LessThanOrEqualTo   ] = "<=",
            [OperationType.GreaterThanOrEqualTo] = ">=",
        }.ToImmutableDictionary();

        public override (string?, string?, string?, string?) ToQuad() =>
            (operationMap[Operation], Operand1.ToString(), Operand2.ToString(), Target.ToString());
    }
}
