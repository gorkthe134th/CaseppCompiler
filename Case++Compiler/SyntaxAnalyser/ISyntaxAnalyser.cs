using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser;

namespace CaseppCompiler.SyntaxAnalyser
{
    public interface ISyntaxAnalyser
    {
        public void Analyse(IEnumerable<Token> input);
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
