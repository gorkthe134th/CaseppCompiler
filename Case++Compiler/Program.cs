using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using System.Collections.Concurrent;

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
            
            Stream input = File.OpenRead(args[0]);

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

            using CancellationTokenSource cancellationTokenSource = new();
            using TokenStream tokens = new(128, cancellationTokenSource.Token);
            using IntermediateProgram program = new(functionCapacity: 4);

            IList<Task> compilationTasks = [
                Task.Run(() => lexicalAnalyser.Analyse(input , tokens ), cancellationTokenSource.Token),
                Task.Run(() => syntaxAnalyser .Analyse(tokens, program), cancellationTokenSource.Token),
            ];

            while (compilationTasks.Count > 0)
            {
                int finishedTaskIndex = Task.WaitAny([.. compilationTasks]);
                Task finishedTask = compilationTasks[finishedTaskIndex];
                compilationTasks.RemoveAt(finishedTaskIndex);

                if (finishedTask.IsFaulted)
                {
                    foreach (var e in finishedTask.Exception.InnerExceptions)
                    {
                        int tabCount = 0;
                        Exception? exception = e;
                        while (exception != null)
                        {
                            Console.WriteLine($"{new string('\t', tabCount++)}{exception.GetType().Name}: {exception.Message}");
                            exception = exception.InnerException;
                        }
                    }
                    cancellationTokenSource.Cancel();
                    try
                    {
                        Task.WaitAll(compilationTasks);
                    }
                    catch (AggregateException e)
                    {
                        foreach (var exception in e.InnerExceptions) if (exception is not OperationCanceledException) throw;
                        // No need to log which Tasks were Canceled; the main Exception is enough
                    }
                    return;
                }
            }

            using StreamWriter writer = new("a.int", false);
            int line = 0;
            foreach (var quad in program.ToQuads())
                writer.WriteLine($"{line++}: {quad.Item1 ?? "_"}, {quad.Item2 ?? "_"}, {quad.Item3 ?? "_"}, {quad.Item4 ?? "_"}");
        }
    }
}