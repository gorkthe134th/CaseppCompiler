using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser
{
    public interface ISyntaxAnalyser
    {
        public void Analyse(TokenStream input, IntermediateProgram? output = null);
    }

    public static class SyntaxAnalyserFactory
    {
        public static ISyntaxAnalyser Create(string type = "") =>
            type switch
            {
                "grammar" => new GrammarSyntaxAnalyserImplementation(),
                _ => new GrammarSyntaxAnalyserImplementation(),
            };
    }
}
