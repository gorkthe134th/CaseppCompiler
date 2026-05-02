namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class InputInstruction(Position Position, Variable Variable) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("in", Variable.Name, null, null);
    }
}
