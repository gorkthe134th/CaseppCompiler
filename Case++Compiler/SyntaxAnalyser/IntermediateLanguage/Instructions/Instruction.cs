namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal abstract record class Instruction(Position Position)
    {
        public abstract (string?, string?, string?, string?) ToQuad();
    }
}
