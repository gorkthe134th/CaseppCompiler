using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

using System.Collections.Immutable;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class OperationInstruction(int line, int column,
        OperatorToken.OperationType operation, Value operand1, Value operand2, VariableSymbol result)
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
            (operationMap[operation], operand1.ToString(), operand2.ToString(), result.Name);
    }
}
