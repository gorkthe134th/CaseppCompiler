using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal class CallInstruction(int line, int column, FunctionSymbol function) : Instruction(line, column)
    {
        public override (string?, string?, string?, string?) ToQuad() => ("call", function.Name, null, null);
    }
}
