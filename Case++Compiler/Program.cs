using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1) throw new ArgumentException("At least one argument is required. Please provide the name of the file to compile.");
            LexicalAnalyserImplementation lexicalAnalyser = new();

            Stream inputStream = File.OpenRead(args[0]);

            try
            {
                foreach (Token token in lexicalAnalyser.Analyse(inputStream)) Console.WriteLine(token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}