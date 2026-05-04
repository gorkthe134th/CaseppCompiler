using CaseppCompiler;
using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using NUnit.Framework.Constraints;

using System.Collections.Concurrent;

namespace CaseppCompilerTest
{
    [TestFixture("regex")]
    [TestFixture("set")]
    public class LexicalAnalyserTests(string type)
    {
        private ILexicalAnalyser analyser;
        private static readonly object[] happyTests =
        [
            new object[] { "EmptyFile.c++",
                "Matcher" %
                [ -"",
                    "EOF" % typeof(EOFToken),
                ]
            },
            new object[] { "SingleLineComment.c++",
                "Matcher" %
                [ -"",
                    "Divide" % OperationType.Divide,
                    "Divide" % OperationType.Divide,
                    "EOF" % typeof(EOFToken),
                ]
            },
            new object[] { "MultiLineComment.c++",
                "Matcher" %
                [ -"",
                    "Multiply" % OperationType.Multiply,
                    "Divide" % OperationType.Divide,
                    "EOF" % typeof(EOFToken),
                ]
            },
            new object[] { "Identifier.c++",
                "Matcher" %
                [ -"",
                    "And1" % OperationType.And,
                    "Not1" % OperationType.Not,
                    "Or1" % OperationType.Or,
                    "Program" % typeof(ProgramToken),
                    new IdentifierTokenMatcher("andnot"),
                    new IdentifierTokenMatcher("ornot"),
                    new IdentifierTokenMatcher("programnot"),
                    new ConstantTokenMatcher("2(1)", 2),
                    new IdentifierTokenMatcher("a2"),
                    new ConstantTokenMatcher("2(2)", 2),
                    new IdentifierTokenMatcher("and2"),
                    new ConstantTokenMatcher("2(3)", 2),
                    new IdentifierTokenMatcher("program2"),
                    new IdentifierTokenMatcher("a23456789012345678901234567890"),
                    "And3" % OperationType.And,
                    "EOF" % typeof(EOFToken),
                ]
            },
            new object[] { "Keyword.c++",
                "Matcher" %
                [ -"",
                    "Program" % typeof(ProgramToken),
                    "Declare" % typeof(DeclareToken),
                    "Function" % typeof(FunctionToken),
                    "In" % typeof(InToken),
                    "Out" % typeof(OutToken),
                    "InOut" % typeof(InOutToken),
                    "Return" % typeof(ReturnToken),
                    "If" % typeof(IfToken),
                    "Else" % typeof(ElseToken),
                    "While" % typeof(WhileToken),
                    "SwitchCase" % typeof(SwitchCaseToken),
                    "InCase" % typeof(InCaseToken),
                    "WhileCase" % typeof(WhileCaseToken),
                    "UntilCase" % typeof(UntilCaseToken),
                    "Until" % typeof(UntilToken),
                    "ForCase" % typeof(ForCaseToken),
                    "When" % typeof(WhenToken),
                    "Default" % typeof(DefaultToken),
                    "Input" % typeof(InputToken),
                    "Print" % typeof(PrintToken),
                    new BoolConstantTokenMatcher("True", true),
                    new BoolConstantTokenMatcher("False", false),
                    "EOF" % typeof(EOFToken),
                ]
            },
            new object[] { "NoCarriageReturn.c++",
                "Matcher" %
                [ -"",
                    new IdentifierTokenMatcher("a"),
                    new ConstantTokenMatcher("1", 1),
                    "+" % OperationType.Add,
                    ">" % OperationType.GreaterThan,
                    "}" % typeof(BlockToken),
                    new IdentifierTokenMatcher("a23456789012345678901234567890"),
                    new IdentifierTokenMatcher("a23456789012345678901234567890"),
                    "EOF" % typeof(EOFToken),
                ]
            },
        ];
        private static readonly object[] sadTests =
        [
            new object[] { "InvalidCharacter.c++"  , new IResolveConstraint[] { Does.StartWith("Line 1, Column 1: Invalid Token") } },
            new object[] { "ConstantOutOfRange.c++", new IResolveConstraint[] { Is.EqualTo("Line 1, Column 1: Constants must be in range [-32767, 32767].") } },
        ];

        [SetUp]
        public void Setup() => analyser = LexicalAnalyserFactory.Create(type);

        [TestCaseSource(nameof(happyTests))]
        public void HappyTest(string file, TokenMatcher matcher)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"LexicalAnalyserTests\Happy\{file}");
            using TokenStream tokenQueue = new();

            analyser.Analyse(File.OpenRead(path), tokenQueue);

            var tokens = tokenQueue.GetConsumingEnumerable().GetEnumerator();
            Assert.That(tokens.MoveNext(), Is.True);
            Assert.That(matcher.TryMatch(tokens, null), Is.True);
        }

        [TestCaseSource(nameof(sadTests))]
        public void SadTest(string file, IEnumerable<IResolveConstraint> messageConstraints)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"LexicalAnalyserTests\Sad\{file}");

            Exception? e = Assert.Throws<LexicalAnalyserException>(() => analyser.Analyse(File.OpenRead(path)), $"Expected LexicalAnalyserException.");
            foreach (var messageConstraint in messageConstraints)
            {
                Assert.That(e, Is.Not.Null, $"Expected more Inner Exceptions.");
                Assert.That(e.Message, messageConstraint);
                e = e.InnerException;
            }
        }

        private class IdentifierTokenMatcher(string name) : TokenMatcher(name)
        {
            public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
            {
                if (tokens.Current is not IdentifierToken identifier || identifier.Name != Name) return false;

                Position currentPosition = tokens.Current.Position;
                if (!tokens.MoveNext()) throw new LexicalAnalyserException(currentPosition + 1, $"Expected EOF Token");

                return true;
            }
        }

        private class ConstantTokenMatcher(string name, int c) : TokenMatcher(name)
        {
            public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
            {
                if (tokens.Current is not ConstantToken constant || constant.Constant != c) return false;

                Position currentPosition = tokens.Current.Position;
                if (!tokens.MoveNext()) throw new LexicalAnalyserException(currentPosition, $"Expected EOF Token");

                return true;
            }
        }

        private class BoolConstantTokenMatcher(string name, bool c) : TokenMatcher(name)
        {
            public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
            {
                if (tokens.Current is not BoolConstantToken constant || constant.Constant != c) return false;

                Position currentPosition = tokens.Current.Position;
                if (!tokens.MoveNext()) throw new LexicalAnalyserException(currentPosition, $"Expected EOF Token");

                return true;
            }
        }
    }
}
