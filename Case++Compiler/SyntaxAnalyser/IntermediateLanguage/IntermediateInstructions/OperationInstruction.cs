using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal class OperationInstruction(int line, int column,
        OperatorToken.OperationType operation, object operand1ID, object operand2ID, string tempID)
        : Instruction(line, column)
    {
        private static readonly Dictionary<OperatorToken.OperationType, string> operationMap = new()
        {
            [OperatorToken.OperationType.Add     ] = "+",
            [OperatorToken.OperationType.Subtract] = "-",
            [OperatorToken.OperationType.Multiply] = "*",
            [OperatorToken.OperationType.Divide  ] = "/",
        };

        public override string ToString() => $"{operationMap[operation]}, {operand1ID}, {operand2ID}, {tempID}";
    }
}
