using CaseppCompiler.CodeGenerator;
using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.Writers.CodeWriter;
using CaseppCompiler.Writers.IntermediateProgramWriter;

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

            using Stream intFile  = File.Open("a.int", FileMode.Create);
            using Stream symFile  = File.Open("a.sym", FileMode.Create);
            using Stream codeFile = File.Open("a.asm", FileMode.Create);

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
                codeWriter     .Write  (code   , codeFile, cancellationTokenSource.Token),
                scopeWriter    .Write  (program, symFile , cancellationTokenSource.Token),
                functionWriter .Write  (program, intFile , cancellationTokenSource.Token),
                codeGenerator  .Analyse(program, code    , cancellationTokenSource.Token),
                syntaxAnalyser .Analyse(tokens , program , cancellationTokenSource.Token),
                lexicalAnalyser.Analyse(inFile , tokens  , cancellationTokenSource.Token),
            ];

            while (compilationTasks.Count > 0)
            {
                try
                {
                    Task finishedTask = await Task.WhenAny([.. compilationTasks]).ConfigureAwait(false);
                    if (!finishedTask.IsCanceled) await finishedTask; // Propagate Exceptions
                    compilationTasks.Remove(finishedTask);
                }
                catch (OperationCanceledException) { } // Ignore OperationCanceledException
                catch
                {
                    if (!cancellationTokenSource.IsCancellationRequested)
                        cancellationTokenSource.Cancel();

                    compilationTasks.RemoveAll(task =>
                    {
                        if (!task.IsFaulted) return false;

                        foreach (var e in task.Exception.Flatten().InnerExceptions)
                        {
                            if (e is OperationCanceledException) continue;

                            int tabCount = 0;
                            Exception? exception = e;
                            while (exception != null)
                            {
                                Console.WriteLine($"{new string('\t', tabCount++)}{exception.GetType().Name}: {exception.Message}");
                                exception = exception.InnerException;
                            }
                        }

                        return true;
                    });
                }
            }
        }
    }
}