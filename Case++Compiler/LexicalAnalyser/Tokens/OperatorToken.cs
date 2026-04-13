using System.Collections.Immutable;
using System.ComponentModel;

namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    public class OperatorToken : Token
    {
        private static readonly ImmutableDictionary<string, OperationType> operationMap = new Dictionary<string, OperationType>()
        {
            ["not"] = OperationType.Not,
            [ "+" ] = OperationType.Add,
            [ "-" ] = OperationType.Subtract,
            [ "*" ] = OperationType.Multiply,
            [ "/" ] = OperationType.Divide,
            [ "=" ] = OperationType.EqualTo,
            [ "<" ] = OperationType.LessThan,
            [ ">" ] = OperationType.GreaterThan,
            ["<>" ] = OperationType.NotEqualTo,
            ["<=" ] = OperationType.LessThanOrEqualTo,
            [">=" ] = OperationType.GreaterThanOrEqualTo,
            ["and"] = OperationType.And,
            ["or" ] = OperationType.Or,
        }.ToImmutableDictionary();

        public static ImmutableDictionary<OperationType, OperationCategory> CategoryMap { get; } = new Dictionary<OperationType, OperationCategory>()
        {
            [OperationType.Not                 ] = OperationCategory.Logical,
            [OperationType.Add                 ] = OperationCategory.Numerical,
            [OperationType.Subtract            ] = OperationCategory.Numerical,
            [OperationType.Multiply            ] = OperationCategory.Numerical,
            [OperationType.Divide              ] = OperationCategory.Numerical,
            [OperationType.EqualTo             ] = OperationCategory.Comparison,
            [OperationType.LessThan            ] = OperationCategory.Comparison,
            [OperationType.GreaterThan         ] = OperationCategory.Comparison,
            [OperationType.NotEqualTo          ] = OperationCategory.Comparison,
            [OperationType.LessThanOrEqualTo   ] = OperationCategory.Comparison,
            [OperationType.GreaterThanOrEqualTo] = OperationCategory.Comparison,
            [OperationType.And                 ] = OperationCategory.Logical,
            [OperationType.Or                  ] = OperationCategory.Logical,
        }.ToImmutableDictionary();

        public enum OperationCategory
        {
            Logical,
            Numerical,
            Comparison,
        }

        public enum OperationType
        {
            Not,
            Add,
            Subtract,
            Multiply,
            Divide,
            EqualTo,
            LessThan,
            GreaterThan,
            NotEqualTo,
            LessThanOrEqualTo,
            GreaterThanOrEqualTo,
            And,
            Or,
        }

        public OperatorToken(string operation, int line, int column) : base(line, column)
        {
            Operation = operationMap.TryGetValue(operation, out OperationType op) ? op :
                throw new LexicalAnalyserException($"{base.ToString()} Invalid Operation \"{operation}\"");
        }

        public OperationType Operation { get; }

        public override string ToString() => $"{base.ToString()} {Operation}";
    }
}
