using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using System.Collections.Concurrent;

namespace CaseppCompiler
{
    internal class Program
    {
        private record CompilationTask(Task Task, string ErrorMessagePrefix);

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

            using BlockingCollection<Token> tokens = new(new ConcurrentQueue<Token>(), boundedCapacity: 128);
            using IntermediateProgram program = new(functionCapacity: 4);

            IList<CompilationTask> compilationTasks = [
                new(Task.Run(() => lexicalAnalyser.Analyse(input , tokens )), "Lexical Analyser Exception"),
                new(Task.Run(() => syntaxAnalyser .Analyse(tokens, program)),  "Syntax Analyser Exception"),
            ];

            while (compilationTasks.Count > 0)
            {
                int finishedTaskIndex = Task.WaitAny([.. compilationTasks.Select(ct => ct.Task)]);
                CompilationTask finishedTask = compilationTasks[finishedTaskIndex];
                compilationTasks.RemoveAt(finishedTaskIndex);
                AggregateException? aggregateException = finishedTask.Task.Exception;
                if (aggregateException != null)
                {
                    foreach (var exception in aggregateException.InnerExceptions)
                        Console.WriteLine($"{finishedTask.ErrorMessagePrefix}: {exception.Message}");
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