namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal record JumpBlockInfo(List<int> TrueOriginList, List<int> FalseOriginList, int Start);
}
