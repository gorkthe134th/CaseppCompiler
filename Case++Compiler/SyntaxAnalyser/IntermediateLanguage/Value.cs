using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    // TODO: Replace with a union when C# 15 is officially released
    internal class Value
    {
        private readonly object _value;

        public Value(uint constant) => _value = constant;

        public Value(Variable variable) => _value = variable;

        internal uint Constant => _value is uint c ? c : throw new InvalidOperationException($"Expected Constant: {_value}");

        internal Variable Variable => _value is Variable v ? v : throw new InvalidOperationException($"Expected Variable: {_value}");

        internal bool IsConstant([NotNullWhen(true)] out uint? constant)
        {
            if (_value is uint c)
            {
                constant = c;
                return true;
            }
            constant = null;
            return false;
        }

        internal bool IsVariable([NotNullWhen(true)] out Variable? variable)
        {
            if (_value is Variable v)
            {
                variable = v;
                return true;
            }
            variable = null;
            return false;
        }

        public static implicit operator uint(Value value) => value.Constant;

        public static implicit operator Variable(Value value) => value.Variable;

        public static implicit operator Value(uint constant) => new(constant);

        public static implicit operator Value(Variable variable) => new(variable);

        public override string ToString() =>
            _value is Variable v ? v.Name : _value is uint c ? c.ToString() :
            throw new InvalidOperationException($"Invalid state for value: {_value}");

        internal static bool TryCast(object obj, [NotNullWhen(true)] out Value? result)
        {
            if (obj is uint c) { result = new Value(c); return true; }
            if (obj is Variable v) { result = new Value(v); return true; }
            result = null;
            return false;
        }
    }
}
