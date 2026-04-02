namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal abstract class JumpInstruction(int line, int column, int? target) : Instruction(line, column)
    {
        public int? Target { get; set; } = target;
    }
}
