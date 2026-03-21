namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal abstract class JumpInstruction(int line, int column) : Instruction(line, column)
    {
        public int? Target { get; set; } = null;

        public override string ToString() => $"{Target}";
    }
}
