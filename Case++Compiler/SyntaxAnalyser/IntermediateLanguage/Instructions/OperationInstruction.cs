using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Collections.Immutable;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class OperationInstruction(Position Position, OperationType Operation, Value Operand1, Value Operand2, Variable Result) : Instruction(Position)
    {
        private static readonly ImmutableDictionary<OperationType, string> operationMap = new Dictionary<OperationType, string>()
        {
            [OperationType.Add     ] = "+",
            [OperationType.Subtract] = "-",
            [OperationType.Multiply] = "*",
            [OperationType.Divide  ] = "/",
        }.ToImmutableDictionary();

        public override (string?, string?, string?, string?) ToQuad() =>
            (operationMap[Operation], Operand1.ToString(), Operand2.ToString(), Result.Name);
    }
}
