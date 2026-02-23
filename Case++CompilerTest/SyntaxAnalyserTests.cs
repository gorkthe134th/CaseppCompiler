using CaseppCompiler.SyntaxAnalyser;
using CaseppCompiler.LexicalAnalyser;

namespace CaseppCompilerTest
{
    [TestFixture("grammar")]
    public class SyntaxAnalyserTests(string type)
    {
        private ISyntaxAnalyser syntaxAnalyser;
        private ILexicalAnalyser lexicalAnalyser;
        private static readonly object[] happyTests =
        [
            new object[] { @"Program\EmptyProgram.c++" },
            new object[] { @"Declarations\EmptyDeclaration.c++" },
            new object[] { @"Declarations\SingleDeclaration.c++" },
            new object[] { @"Declarations\MultiDeclaration.c++" },
            new object[] { @"Functions\EmptyFunction.c++" },
            new object[] { @"Functions\FunctionParameters.c++" },
            new object[] { @"Functions\NestedFunctions.c++" },
            new object[] { @"Expressions\Expression.c++" },
            new object[] { @"Conditions\Condition.c++" },
            new object[] { @"Statements\Assignment\SimpleAssignment.c++" },
            new object[] { @"Statements\OptionalSemiColon.c++" },
        ];
        private static readonly object[] sadTests =
        [
            new object[] { @"EmptyFile.c++", Is.EqualTo("Expected Program: Line 1 Column 1: EOF") },
            new object[] { @"Program\NoKeyword.c++", Is.EqualTo("Expected Program: Line 1 Column 1: Identifier \"progra\"") },
            new object[] { @"Program\NoID.c++", Is.EqualTo("Expected Program ID: Line 1 Column 9: Block Start") },
            new object[] { @"Program\NoBlockEnd.c++", Is.EqualTo("Expected Block End Token: Line 1 Column 17: EOF") },
            new object[] { @"Declarations\NoSemiColon.c++", Is.EqualTo("Expected Semi Colon: Line 4 Column 1: Block End") },
            new object[] { @"Declarations\NoSemiColonBetween.c++", Is.EqualTo("Expected Semi Colon: Line 4 Column 2: Declare") },
            new object[] { @"Declarations\NoCommaBetween.c++", Is.EqualTo("Expected Semi Colon: Line 3 Column 12: Identifier \"y\"") },
            new object[] { @"Functions\NoID.c++", Is.EqualTo("Expected Function ID: Line 3 Column 11: Parenthesis Start") },
            new object[] { @"Functions\NoParameters.c++", Is.EqualTo("Expected Formal Parameter List: Line 3 Column 13: Block Start") },
            new object[] { @"Functions\NoBody.c++", Is.EqualTo("Expected Body: Line 4 Column 1: Block End") },
            new object[] { @"Functions\Parameters\NoIDIn.c++", Is.EqualTo("Expected Parameter ID: Line 3 Column 15: Parenthesis End") },
            new object[] { @"Functions\Parameters\NoIDOut.c++", Is.EqualTo("Expected Parameter ID: Line 3 Column 16: Parenthesis End") },
            new object[] { @"Functions\Parameters\NoIDInOut.c++", Is.EqualTo("Expected Parameter ID: Line 3 Column 18: Parenthesis End") },
            new object[] { @"Functions\Parameters\OnlyComma.c++", Is.EqualTo("Expected Close Parenthesis Token: Line 3 Column 13: Comma") },
            new object[] { @"Functions\Parameters\TrailingComma.c++", Is.EqualTo("Expected Formal Parameter: Line 3 Column 15: Parenthesis End") },
            new object[] { @"Expressions\MultiplyFirst.c++", Is.EqualTo("Expected Expression: Line 3 Column 7: Multiply") },
            new object[] { @"Expressions\NoOperator.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 9: Constant 9") },
            new object[] { @"Expressions\DoubleOperator.c++", Is.EqualTo("Expected Term: Line 3 Column 9: Add") },
            new object[] { @"Expressions\ParenthesisAfterConstant.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 8: Parenthesis Start") },
            new object[] { @"Expressions\IdentifierAfterConstant.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 8: Identifier \"x\"") },
            new object[] { @"Expressions\NoCloseParenthesis.c++", Is.EqualTo("Expected Close Parenthesis Token: Line 3 Column 9: Semi Colon") },
            new object[] { @"Statements\Assignment\NoID.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 2: Assignment") },
            new object[] { @"Statements\Assignment\NoAssignment.c++", Is.EqualTo("Expected Assignment Token: Line 3 Column 4: Constant 9") },
            new object[] { @"Statements\Assignment\NoExpression.c++", Is.EqualTo("Expected Expression: Line 4 Column 1: Block End") },
        ];

        [SetUp]
        public void Setup()
        {
            lexicalAnalyser = LexicalAnalyserFactory.Create();
            syntaxAnalyser = SyntaxAnalyserFactory.Create(type);
        }

        [TestCaseSource(nameof(happyTests))]
        public void HappyTest(string file)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"SyntaxAnalyserTests\Happy\{file}");
            syntaxAnalyser.Analyse(lexicalAnalyser.Analyse(File.OpenRead(path)));
        }

        [TestCaseSource(nameof(sadTests))]
        public void SadTest(string file, NUnit.Framework.Constraints.IResolveConstraint messageConstraint)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"SyntaxAnalyserTests\Sad\{file}");
            var e = Assert.Throws<SyntaxAnalyserException>(() => syntaxAnalyser.Analyse(lexicalAnalyser.Analyse(File.OpenRead(path))), $"Expected SyntaxAnalyserException");
            Assert.That(e.Message, messageConstraint);
        }
    }
}
