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

            string lexicalAnalyserType = "";
            string syntaxAnalyserType  = "";
            for (int i = 1; i < args.Length - 1; i++)
            {
                if (args[i].StartsWith("-l")) { lexicalAnalyserType = args[i + 1]; i++; continue; }
                if (args[i].StartsWith("-s")) { syntaxAnalyserType  = args[i + 1]; i++; continue; }
            }

            ILexicalAnalyser lexicalAnalyser = LexicalAnalyserFactory.Create(lexicalAnalyserType);
            ISyntaxAnalyser  syntaxAnalyser  = SyntaxAnalyserFactory .Create(syntaxAnalyserType );

            try
            {
                syntaxAnalyser.Analyse(lexicalAnalyser.Analyse(inputStream).Select(t => { Console.WriteLine(t); return t; }));
                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}