namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal abstract record class Instruction(Position Position)
    {
        public virtual Task Complete => Task.CompletedTask;

        public abstract (string?, string?, string?, string?) ToQuad();
    }
}
