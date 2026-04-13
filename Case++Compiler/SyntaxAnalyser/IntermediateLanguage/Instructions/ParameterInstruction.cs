using System.Collections.Immutable;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class ParameterInstruction(int line, int column, object parameter, ParameterInstruction.ParameterType type) : Instruction(line, column)
    {
        private static readonly ImmutableDictionary<ParameterType, string> typeMap = new Dictionary<ParameterType, string>()
        {
            [ParameterType.In   ] = "cv",
            [ParameterType.Out  ] = "ret",
            [ParameterType.InOut] = "ref",
        }.ToImmutableDictionary();

        public enum ParameterType { In, Out, InOut };

        public override (string?, string?, string?, string?) ToQuad() => ("par", parameter.ToString(), typeMap[type], null);
    }
}
