using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.SyntaxAnalyser;

namespace CaseppCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Write(
                    "At least one argument is required. Please provide the name of the file to compile.\n" +
                    "You may also use -l and -s to control the type of analysers to use. The available options are:\n" +
                    "-l [set | regex] (default is \"set\")\n" +
                    "-s [grammar] (default is \"grammar\")\n" +
                    "Example: Case++Compiler.exe program.c++ -l regex\n");
                return;
            }
            
            Stream inputStream = File.OpenRead(args[0]);

            string lexicalAnalyserType = "";
            string syntaxAnalyserType  = "";
            for (int i = 1; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "-l":
                        lexicalAnalyserType = args[++i];
                        break;
                    case "-s":
                        syntaxAnalyserType = args[++i];
                        break;
                    default:
                        break;
                }
            }

            ILexicalAnalyser lexicalAnalyser = LexicalAnalyserFactory.Create(lexicalAnalyserType);
            ISyntaxAnalyser  syntaxAnalyser  = SyntaxAnalyserFactory .Create(syntaxAnalyserType );

            try
            {
                var tokens = lexicalAnalyser.Analyse(inputStream);
                var intermediateProgram = syntaxAnalyser.Analyse(tokens);
                using StreamWriter writer = new("a.il++", false);
                int line = 0;
                foreach (var quad in intermediateProgram.ToQuads())
                    writer.WriteLine((quad.Item1?.Contains("block") == false ? line++ + ": " : "") +
                        string.Join(", ", quad.Item1 ?? "_", quad.Item2 ?? "_", quad.Item3 ?? "_", quad.Item4 ?? "_"));
            }
            catch (LexicalAnalyserException e)
            {
                Console.WriteLine($"Lexical Analyser Exception: {e.Message}");
            }
            catch (SyntaxAnalyserException e)
            {
                Console.WriteLine($"Syntax Analyser Exception: {e.Message}");
            }
        }
    }
}