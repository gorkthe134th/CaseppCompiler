using CaseppCompiler.CodeGenerator;
using CaseppCompiler.CodeWriter;
using CaseppCompiler.IntermediateProgramWriter;
using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Write(
                    "At least one argument is required. Please provide the name of the file to compile.\n" +
                    "You may also use -l, -s and -c to control the type of analysers to use. The available options are:\n" +
                    "-l [set | regex] (default is \"set\")\n" +
                    "-s [grammar] (default is \"grammar\")\n" +
                    "-c [riscv] (default is \"riscv\")\n" +
                    "Example: Case++Compiler.exe program.c++ -c riscv -l regex\n");
                return;
            }

            using Stream inFile = File.OpenRead(args[0]);

            string lexicalAnalyserType = "";
            string syntaxAnalyserType  = "";
            string codeGeneratorType   = "";
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
                    case "-c":
                        codeGeneratorType = args[++i];
                        break;
                    default:
                        break;
                }
            }

            using Stream intFile  = File.OpenWrite("a.int");
            using Stream symFile  = File.OpenWrite("a.sym");
            using Stream codeFile = File.OpenWrite("a.asm");

            ILexicalAnalyser           lexicalAnalyser = LexicalAnalyserFactory          .Create(lexicalAnalyserType);
            ISyntaxAnalyser            syntaxAnalyser  = SyntaxAnalyserFactory           .Create(syntaxAnalyserType );
            IIntermediateProgramWriter functionWriter  = IntermediateProgramWriterFactory.Create("int");
            IIntermediateProgramWriter scopeWriter     = IntermediateProgramWriterFactory.Create("sym");
            ICodeGenerator             codeGenerator   = CodeGeneratorFactory            .Create(codeGeneratorType  );
            ICodeWriter                codeWriter      = CodeWriterFactory               .Create();

            using CancellationTokenSource cancellationTokenSource = new();
            Stream<Token> tokens = new(capacity: 128, cancellationTokenSource.Token);
            IntermediateProgram program = new(functionCapacity: 4, instructionCapacity: null, scopeCapacity: 6, cancellationTokenSource.Token);
            Stream<string> code = new(capacity: 64, cancellationTokenSource.Token);

            List<Task> compilationTasks = [
                codeWriter     .Write  (code   , codeFile),
                scopeWriter    .Write  (program, symFile ),
                functionWriter .Write  (program, intFile ),
                codeGenerator  .Analyse(program, code    ),
                syntaxAnalyser .Analyse(tokens , program ),
                lexicalAnalyser.Analyse(inFile , tokens  ),
            ];

            while (compilationTasks.Count > 0)
            {
                Task finishedTask = await Task.WhenAny([.. compilationTasks]);
                compilationTasks.Remove(finishedTask);

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
        }
    }
}