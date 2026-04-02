using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class ComparisonJumpInstruction(int line, int column,
        OperatorToken.OperationType operation, object operand1, object operand2, int? target)
        : JumpInstruction(line, column, target)
    {
        private static readonly Dictionary<OperatorToken.OperationType, string> operationMap = new()
        {
            [OperatorToken.OperationType.EqualTo             ] = "=",
            [OperatorToken.OperationType.LessThan            ] = "<",
            [OperatorToken.OperationType.GreaterThan         ] = ">",
            [OperatorToken.OperationType.NotEqualTo          ] = "<>",
            [OperatorToken.OperationType.LessThanOrEqualTo   ] = "<=",
            [OperatorToken.OperationType.GreaterThanOrEqualTo] = ">=",
        };

        public override (string?, string?, string?, string?) ToQuad() =>
            (operationMap[operation], operand1.ToString(), operand2.ToString(), Target.ToString());
    }
}
