namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class AssignmentInstruction(Position Position, Variable Variable, Value Value) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => (":=", Value.ToString(), null, Variable.Name);
    }
}
