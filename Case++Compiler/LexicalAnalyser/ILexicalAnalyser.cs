using CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser;

namespace CaseppCompiler.LexicalAnalyser
{
    public interface ILexicalAnalyser
    {
        public void Analyse(Stream input, TokenStream? output = null);
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
