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

            Task[] compilationTasks = [
                codeWriter     .Write  (code   , codeFile),
                scopeWriter    .Write  (program, symFile ),
                functionWriter .Write  (program, intFile ),
                codeGenerator  .Analyse(program, code    ),
                syntaxAnalyser .Analyse(tokens , program ),
                lexicalAnalyser.Analyse(inFile , tokens  ),
            ];

            try
            {
                await Task.WhenAll(compilationTasks);
            }
            catch
            {
                cancellationTokenSource.Cancel();

                foreach (var task in compilationTasks)
                {
                    if (!task.IsFaulted) continue;

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
                }

                try
                {
                    await Task.WhenAll(compilationTasks);
                    // Make sure every task is finished before disposing any resources
                }
                catch { }
            }
        }
    }
}