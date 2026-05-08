using CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser
{
    public interface ILexicalAnalyser
    {
        public Task Analyse(Stream input, Stream<Token>? output = null);
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
