using System;
using System.Collections.Generic;
using System.Text;

namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class OperatorToken : Token
    {
        private static readonly Dictionary<string, OperationType> operationMap = new()
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
        };

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
                throw new ArgumentException($"{base.ToString()} Invalid Operation \"{operation}\"");
        }

        public OperationType Operation { get; }

        public override string ToString() => $"{base.ToString()} {Operation}";
    }
}
