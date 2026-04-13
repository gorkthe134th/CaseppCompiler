using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Collections.Immutable;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class OperationInstruction(int line, int column,
        OperatorToken.OperationType operation, object operand1, object operand2, string tempID)
        : Instruction(line, column)
    {
        private static readonly ImmutableDictionary<OperatorToken.OperationType, string> operationMap = new Dictionary<OperatorToken.OperationType, string>()
        {
            [OperatorToken.OperationType.Add     ] = "+",
            [OperatorToken.OperationType.Subtract] = "-",
            [OperatorToken.OperationType.Multiply] = "*",
            [OperatorToken.OperationType.Divide  ] = "/",
        }.ToImmutableDictionary();

        public override (string?, string?, string?, string?) ToQuad() =>
            (operationMap[operation], operand1.ToString(), operand2.ToString(), tempID);
    }
}
