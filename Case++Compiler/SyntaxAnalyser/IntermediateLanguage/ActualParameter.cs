namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal abstract record ActualParameter
    {
        public abstract string? Arg1 { get; }
        public abstract string? Arg2 { get; }
    }

    internal record InParameter(Value Value) : ActualParameter
    {
        public override string? Arg1 => Value.ToString();
        public override string? Arg2 => "cv";

        public override string ToString() => $"in {Value}";
    }

    internal record InOutParameter(Variable Variable) : ActualParameter
    {
        public override string? Arg1 => Variable.Name;
        public override string? Arg2 => "ref";

        public override string ToString() => $"inout {Variable.Name}";
    }

    internal record OutParameter(Variable Variable) : ActualParameter
    {
        public override string? Arg1 => Variable.Name;
        public override string? Arg2 => "ret";

        public override string ToString() => $"out {Variable.Name}";
    }
}
