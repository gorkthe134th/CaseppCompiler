namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class ParameterInstruction(int line, int column, object parameter, ParameterInstruction.ParameterType type) : Instruction(line, column)
    {
        private static readonly Dictionary<ParameterType, string> typeMap = new()
        {
            [ParameterType.In   ] = "cv",
            [ParameterType.Out  ] = "ret",
            [ParameterType.InOut] = "ref",
        };

        public enum ParameterType { In, Out, InOut };

        public override (string?, string?, string?, string?) ToQuad() => ("par", parameter.ToString(), typeMap[type], null);
    }
}
