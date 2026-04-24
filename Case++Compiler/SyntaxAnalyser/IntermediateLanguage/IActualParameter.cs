using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal interface IActualParameter
    {
        public string? Arg1 { get; }
        public string? Arg2 { get; }
    }

    internal record InParameter(Value Value) : IActualParameter
    {
        public string? Arg1 => Value.ToString();
        public string? Arg2 => "cv";
    }

    internal record InOutParameter(VariableSymbol Variable) : IActualParameter
    {
        public string? Arg1 => Variable.Name;
        public string? Arg2 => "ref";
    }

    internal record OutParameter(VariableSymbol Variable) : IActualParameter
    {
        public string? Arg1 => Variable.Name;
        public string? Arg2 => "ret";
    }
}
