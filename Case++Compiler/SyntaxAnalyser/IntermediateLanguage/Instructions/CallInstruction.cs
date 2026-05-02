namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal record class CallInstruction(Position Position, Function Function) : Instruction(Position)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("call", Function.Name, null, null);
    }
}
