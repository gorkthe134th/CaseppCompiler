using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal class ComparisonJumpInstruction(int line, int column,
        OperatorToken.OperationType operation, object operand1, object operand2)
        : JumpInstruction(line, column)
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

        public override string ToString() => $"{operationMap[operation]}, {operand1}, {operand2}, {base.ToString()}";
    }
}
