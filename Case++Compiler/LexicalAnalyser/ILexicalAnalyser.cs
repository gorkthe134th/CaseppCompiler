using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser;

using System.Collections.Concurrent;

namespace CaseppCompiler.LexicalAnalyser
{
    public interface ILexicalAnalyser
    {
        public void Analyse(Stream input, BlockingCollection<Token>? output = null);
    }

    public static class LexicalAnalyserFactory
    {
        public static ILexicalAnalyser Create(string type = "") =>
            type switch
            {
                "regex" => new RegexLexicalAnalyserImplementation(),
                "set" => new SetLexicalAnalyserImplementation(),
                _ => new SetLexicalAnalyserImplementation(),
            };
    }
}
