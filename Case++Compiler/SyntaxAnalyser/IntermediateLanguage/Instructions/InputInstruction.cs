using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class InputInstruction(int line, int column, VariableSymbol variable) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("in", variable.Name, null, null);
    }
}
