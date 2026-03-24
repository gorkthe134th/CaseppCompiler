using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser
{
    public interface ISyntaxAnalyser
    {
        public IntermediateProgram Analyse(IEnumerable<Token> input);

        public void Validate(IEnumerable<Token> input);
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
