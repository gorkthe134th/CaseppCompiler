using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser;

namespace CaseppCompiler.LexicalAnalyser
{
    public interface ILexicalAnalyser
    {
        public IEnumerable<Token> Analyse(Stream input);
    }

    public static class LexicalAnalyserFactory
    {
        public static ILexicalAnalyser Create(string type) =>
            type switch
            {
                "regex" => new RegexLexicalAnalyserImplementation(),
                "set" => new SetLexicalAnalyserImplementation(),
                _ => new SetLexicalAnalyserImplementation(),
            };
    }
}
