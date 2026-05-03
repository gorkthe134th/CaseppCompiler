using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.CodeGenerator.RISCVCodeGenerator
{
    internal class StackFrame
    {
        public static int extraBytes = 8; // Include 2 more words for the return address and the parent frame pointer

        public int Length { get; }

        public IDictionary<Variable, int> VariableOffsets { get; }

        public StackFrame(params IEnumerable<Variable> variables)
        {
            int length = 0;
            VariableOffsets = variables.Select((v, i) => new KeyValuePair<Variable, int>(v, length = 4 * i + extraBytes)).ToDictionary();
            Length = length;
        }
    }
}
