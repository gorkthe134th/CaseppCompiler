namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class ParameterInstruction(int line, int column, IActualParameter parameter) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("par", parameter.Arg1, parameter.Arg2, null);
    }
}
