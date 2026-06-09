using CaseppCompiler;
using CaseppCompiler.CodeGenerator;
using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using NUnit.Framework.Constraints;

namespace CaseppCompilerTest.TestClasses
{
    [TestFixture]
    internal class RISCVCodeGeneratorTests
    {
        private ISyntaxAnalyser syntaxAnalyser;
        private ILexicalAnalyser lexicalAnalyser;
        private ICodeGenerator codeGenerator;

        private static readonly object[] happyTests =
        [
            new object[] { @"Empty" },
            new object[] { @"Assignment\Assignment" },
            new object[] { @"Operation\Operation" },
            new object[] { @"Input\Input" },
            new object[] { @"Output\Output" },
            new object[] { @"Jump\Jump" },
            new object[] { @"Parameter\In" },
            new object[] { @"Parameter\InOut" },
            new object[] { @"Parameter\Out" },
            new object[] { @"Parameter\ParameterShadow" }, // This test has unexpected results. Consider adding a Warning.
            new object[] { @"Call\CallFromSelf" },
            new object[] { @"Call\CallFromParent" },
            new object[] { @"Call\CallFromSibling" },
            new object[] { @"Call\CallFromNephew" },
        ];

        private static readonly object[] sadTests =
        [
            new object[] { @"ParameterCountMismatch.c++", new IResolveConstraint[] { Is.EqualTo("Line 9, Column 3: Function \"f\" requires 2 parameters, but got 1.") } },
            new object[] { @"ParameterMismatch.c++", new IResolveConstraint[] { Is.EqualTo("Line 8, Column 3: Function \"f\" actual parameter \"out x\" does not match formal parameter \"In Parameter a\".") } },
            new object[] { @"FunctionReturnNull.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 5: Function \"f\" must return a value.") } },
        ];

        [SetUp]
        public void Setup()
        {
            lexicalAnalyser = LexicalAnalyserFactory.Create();
            syntaxAnalyser = SyntaxAnalyserFactory.Create();
            codeGenerator = CodeGeneratorFactory.Create("riscv");
        }

        [TestCaseSource(nameof(happyTests))]
        public async Task HappyAsync(string name)
        {
            string cppPath = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"RISCVCodeGeneratorTests\Happy\{name}.c++");
            string asmPath = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"RISCVCodeGeneratorTests\Happy\{name}.asm");

            Stream<Token> tokens = new();
            IntermediateProgram program = new();
            Stream<string> code = new();

            await lexicalAnalyser.Analyse(File.OpenRead(cppPath), tokens);
            await syntaxAnalyser.Analyse(tokens, program);
            await codeGenerator.Analyse(program, code);

            var ep = code.GetAsyncEnumerable().GetAsyncEnumerator();
            var ee = ((IEnumerable<string>)File.ReadAllLines(asmPath)).GetEnumerator();
            int line = 0;
            while (true)
                switch ((await ep.MoveNextAsync(), ee.MoveNext()))
                {
                    case (true, true):
                        Assert.That(ep.Current, Is.EqualTo(ee.Current.Split('#', 2, StringSplitOptions.TrimEntries)[0]), $"Difference in line {++line}");
                        continue;
                    case (false, false):
                        return;
                    default:
                        Assert.Fail("Length mismatch");
                        return;
                }
        }

        [TestCaseSource(nameof(sadTests))]
        public void Sad(string file, IEnumerable<IResolveConstraint> messageConstraints)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"RISCVCodeGeneratorTests\Sad\{file}");

            Exception? e = Assert.ThrowsAsync<CodeGeneratorException>(async () =>
            {
                Stream<Token> tokens = new();
                IntermediateProgram program = new();
                Stream<string> code = new();

                await lexicalAnalyser.Analyse(File.OpenRead(path), tokens);
                await syntaxAnalyser.Analyse(tokens, program);
                await codeGenerator.Analyse(program, code);
            },
            $"Expected CodeGeneratorException.");

            foreach (var messageConstraint in messageConstraints)
            {
                Assert.That(e, Is.Not.Null, $"Expected more Inner Exceptions.");
                Assert.That(e.Message, messageConstraint);
                e = e.InnerException;
            }
        }
    }
}
