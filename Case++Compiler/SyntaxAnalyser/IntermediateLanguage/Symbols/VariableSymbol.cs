namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols
{
    internal record class VariableSymbol(string Name, bool Reference) : Symbol(Name) { }
}
