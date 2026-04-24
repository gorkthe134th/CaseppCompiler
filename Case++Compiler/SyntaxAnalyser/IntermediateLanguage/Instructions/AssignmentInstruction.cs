using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class AssignmentInstruction(int line, int column, VariableSymbol variable, Value value) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => (":=", value.ToString(), null, variable.Name);
    }
}
