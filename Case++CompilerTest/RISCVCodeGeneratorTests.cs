using CaseppCompiler.CodeGenerator;
using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using NUnit.Framework.Constraints;

namespace CaseppCompilerTest
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
            new object[] { @"CallFromParent" },
            new object[] { @"CallFromSibling" },
            new object[] { @"CallFromNephew" },
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
        public void Happy(string name)
        {
            string cppPath = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"RISCVCodeGeneratorTests\Happy\{name}.c++");
            string asmPath = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"RISCVCodeGeneratorTests\Happy\{name}.asm");

            using TokenStream tokens = new();
            using IntermediateProgram program = new();
            using CodeStream code = new();

            lexicalAnalyser.Analyse(File.OpenRead(cppPath), tokens);
            syntaxAnalyser.Analyse(tokens, program);
            codeGenerator.Analyse(program, code);

            var ep = code.GetConsumingEnumerable().GetEnumerator();
            var ee = ((IEnumerable<string>)File.ReadAllLines(asmPath)).GetEnumerator();
            int line = 0;
            while (true)
                switch ((ep.MoveNext(), ee.MoveNext()))
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

            Exception? e = Assert.Throws<CodeGeneratorException>(() =>
            {
                using TokenStream tokens = new();
                using IntermediateProgram program = new();
                using CodeStream code = new();

                lexicalAnalyser.Analyse(File.OpenRead(path), tokens);
                syntaxAnalyser.Analyse(tokens, program);
                codeGenerator.Analyse(program, code);
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
