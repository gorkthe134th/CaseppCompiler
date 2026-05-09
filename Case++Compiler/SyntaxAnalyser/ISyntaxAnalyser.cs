using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser
{
    public interface ISyntaxAnalyser
    {
        public Task Analyse(Stream<Token> input, IntermediateProgram? output = null, CancellationToken? cancellationToken = null);
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
