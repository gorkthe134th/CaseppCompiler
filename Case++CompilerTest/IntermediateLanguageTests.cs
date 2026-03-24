using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompilerTest
{
    [TestFixture("grammar")]
    public class IntermediateLanguageTests(string type)
    {
        private ISyntaxAnalyser syntaxAnalyser;
        private ILexicalAnalyser lexicalAnalyser;
        private static readonly object[] tests =
        [
            new object[] { @"Program\EmptyProgram.c++", new (string?, string?, string?, string?)[] {
                ("begin_block", "p", null, null),
                ("halt", null, null, null),
                ("end_block", "p", null, null),
            } },
            new object[] { @"Functions\EmptyFunction.c++", new (string?, string?, string?, string?)[] {
                ("begin_block", "f", null, null),
                ("end_block", "f", null, null),
                ("begin_block", "p", null, null),
                ("halt", null, null, null),
                ("end_block", "p", null, null),
            } },
            new object[] { @"Functions\NestedFunctions.c++", new (string?, string?, string?, string?)[] {
                ("begin_block", "h_g_f", null, null),
                ("end_block", "h_g_f", null, null),
                ("begin_block", "g_f", null, null),
                ("end_block", "g_f", null, null),
                ("begin_block", "f", null, null),
                ("end_block", "f", null, null),
                ("begin_block", "p", null, null),
                ("halt", null, null, null),
                ("end_block", "p", null, null),
            } },
        ];

        [SetUp]
        public void Setup()
        {
            lexicalAnalyser = LexicalAnalyserFactory.Create();
            syntaxAnalyser = SyntaxAnalyserFactory.Create(type);
        }

        [TestCaseSource(nameof(tests))]
        public void Test(string file, (string?, string?, string?, string?)[] expectedQuads)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"IntermediateLanguageTests\{file}");
            IntermediateProgram program = syntaxAnalyser.Analyse(lexicalAnalyser.Analyse(File.OpenRead(path)));
            Assert.That(program.ToQuads().SequenceEqual(expectedQuads));
        }
    }
}
