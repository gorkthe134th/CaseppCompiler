using CaseppCompiler.Tokens;

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace CaseppCompiler
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1) throw new ArgumentException("At least one argument is required. Please provide the name of the file to compile.");
            LexicalAnalyser lexicalAnalyser = new();

            Stream inputStream = File.OpenRead(args[0]);

            try
            {
                foreach (Token token in lexicalAnalyser.Analyse(inputStream)) Console.WriteLine($"Line {token.Line}: {token.Text}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}