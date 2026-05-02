using System.Collections.Immutable;

namespace CaseppCompiler
{
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

    public enum OperationCategory
    {
        Logical,
        Numerical,
        Comparison,
    }

    public static class OperationTypeExtensions
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

        private static readonly ImmutableDictionary<OperationType, OperationCategory> categoryMap = new Dictionary<OperationType, OperationCategory>()
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

        extension(OperationType operation)
        {
            public OperationCategory Category => categoryMap[operation];
            
            public static OperationType FromSymbol(string operationSymbol) =>
                operationMap.TryGetValue(operationSymbol, out OperationType op) ? op :
                    throw new InvalidOperationException($"Invalid Operation \"{operationSymbol}\"");
        }
    }
}
