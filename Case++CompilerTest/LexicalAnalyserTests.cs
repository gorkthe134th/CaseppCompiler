using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers;

namespace CaseppCompilerTest
{
    [TestFixture("regex")]
    [TestFixture("set")]
    public class LexicalAnalyserTests(string type)
    {
        private ILexicalAnalyser analyser;
        private static readonly object[] tests =
        [
            new object[] { "EmptyFile.c++", "EOF" % typeof(EOFToken) },
            new object[] { "SingleLineCommentAtEnd.c++",
                "Matcher" %
                [ -"",
                    "EOF" % typeof(EOFToken),
                ]
            },
            new object[] { "MultiLineComment.c++",
                "Matcher" %
                [ -"",
                    "Multiply" % OperatorToken.OperationType.Multiply,
                    "Divide" % OperatorToken.OperationType.Divide,
                    "EOF" % typeof(EOFToken),
                ]
            },
        ];

        [SetUp]
        public void Setup() => analyser = LexicalAnalyserFactory.Create(type);

        [TestCaseSource(nameof(tests))]
        public void Test(string file, TokenMatcher matcher)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"LexicalAnalyserTests\{file}");
            var tokens = analyser.Analyse(File.OpenRead(path)).GetEnumerator();
            Assert.That(tokens.MoveNext(), Is.True);
            Assert.That(matcher.TryMatch(tokens), Is.True);
        }
    }
}
