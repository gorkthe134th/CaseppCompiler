using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser;

namespace CaseppCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1) throw new ArgumentException("At least one argument is required. Please provide the name of the file to compile.");
            
            Stream inputStream = File.OpenRead(args[0]);
            ILexicalAnalyser lexicalAnalyser = LexicalAnalyserFactory.Create(args.Length > 1 ? args[1] : "");
            ISyntaxAnalyser syntaxAnalyser = SyntaxAnalyserFactory.Create("");

            try
            {
                syntaxAnalyser.Analyse(lexicalAnalyser.Analyse(inputStream).Select(t => { Console.WriteLine(t); return t; }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}