namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class ParameterInstruction(Position Position, ActualParameter Parameter) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("par", Parameter.Arg1, Parameter.Arg2, null);
    }
}
