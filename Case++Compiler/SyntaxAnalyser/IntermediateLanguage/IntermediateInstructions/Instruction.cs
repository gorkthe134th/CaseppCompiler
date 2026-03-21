namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions
{
    internal abstract class Instruction(int line, int column)
    {
        public int Line { get; } = line;

        public int Column { get; } = column;

        public override string ToString() => "$Invalid Instruction$";
    }
}
