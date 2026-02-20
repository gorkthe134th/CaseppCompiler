using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;
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
            new object[] { "EmptyFile.c++",
                "Matcher" %
                [ -"",
                    "EOF" % typeof(EOFToken),
                ]
            },
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
            new object[] { "Identifier.c++",
                "Matcher" %
                [ -"",
                    "And1" % OperatorToken.OperationType.And,
                    "Not1" % OperatorToken.OperationType.Not,
                    "Or1" % OperatorToken.OperationType.Or,
                    "Program" % typeof(ProgramToken),
                    new IdentifierTokenMatcher("andnot"),
                    new IdentifierTokenMatcher("ornot"),
                    new IdentifierTokenMatcher("programnot"),
                    new ConstantTokenMatcher("2(1)", 2),
                    "And2" % OperatorToken.OperationType.And,
                    new ConstantTokenMatcher("2(2)", 2),
                    new ConstantTokenMatcher("2(3)", 2),
                    new IdentifierTokenMatcher("program2"),
                    new IdentifierTokenMatcher("a23456789012345678901234567890"),
                    "And3" % OperatorToken.OperationType.And,
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

        private class IdentifierTokenMatcher(string name) : TokenMatcher(name)
        {
            public override bool? TryMatch(IEnumerator<Token> tokens)
            {
                if (tokens.Current is not IdentifierToken identifier || identifier.Name != Name) return false;

                if (!tokens.MoveNext()) throw new ArgumentException($"Expected EOF Token");

                return true;
            }
        }

        private class ConstantTokenMatcher(string name, int c) : TokenMatcher(name)
        {
            public override bool? TryMatch(IEnumerator<Token> tokens)
            {
                if (tokens.Current is not ConstantToken constant || constant.Constant != c) return false;

                if (!tokens.MoveNext()) throw new ArgumentException($"Expected EOF Token");

                return true;
            }
        }
    }
}
