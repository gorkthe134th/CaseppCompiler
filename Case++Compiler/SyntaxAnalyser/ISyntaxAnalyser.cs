using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using System.Collections.Concurrent;

namespace CaseppCompiler.SyntaxAnalyser
{
    public interface ISyntaxAnalyser
    {
        public void Analyse(BlockingCollection<Token> input, IntermediateProgram? output = null);
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
