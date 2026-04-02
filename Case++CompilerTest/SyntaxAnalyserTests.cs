using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser;

using System.Collections.Concurrent;

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
            new object[] { @"Program\SingleSatementProgram.c++" },
            new object[] { @"Declarations\EmptyDeclaration.c++" },
            new object[] { @"Declarations\SingleDeclaration.c++" },
            new object[] { @"Declarations\MultiDeclaration.c++" },
            new object[] { @"Functions\EmptyFunction.c++" },
            new object[] { @"Functions\SingleSatementFunction.c++" },
            new object[] { @"Functions\FunctionParameters.c++" },
            new object[] { @"Functions\NestedFunctions.c++" },
            new object[] { @"Expressions\Expression.c++" },
            new object[] { @"Conditions\Condition.c++" },
            new object[] { @"Statements\OptionalSemiColon.c++" },
            new object[] { @"Statements\Assignment\SimpleAssignment.c++" },
            new object[] { @"Statements\If\EmptyIfNoElse.c++" },
            new object[] { @"Statements\If\EmptyIfEmptyElse.c++" },
            new object[] { @"Statements\If\EmptyIfSingleElse.c++" },
            new object[] { @"Statements\If\EmptyIfBlockElse.c++" },
            new object[] { @"Statements\If\SingleIfNoElse.c++" },
            new object[] { @"Statements\If\SingleIfEmptyElse.c++" },
            new object[] { @"Statements\If\SingleIfSingleElse.c++" },
            new object[] { @"Statements\If\SingleIfBlockElse.c++" },
            new object[] { @"Statements\If\BlockIfNoElse.c++" },
            new object[] { @"Statements\If\BlockIfEmptyElse.c++" },
            new object[] { @"Statements\If\BlockIfSingleElse.c++" },
            new object[] { @"Statements\If\BlockIfBlockElse.c++" },
            new object[] { @"Statements\While\EmptyWhileNoElse.c++" },
            new object[] { @"Statements\While\EmptyWhileEmptyElse.c++" },
            new object[] { @"Statements\While\EmptyWhileSingleElse.c++" },
            new object[] { @"Statements\While\EmptyWhileBlockElse.c++" },
            new object[] { @"Statements\While\SingleWhileNoElse.c++" },
            new object[] { @"Statements\While\SingleWhileEmptyElse.c++" },
            new object[] { @"Statements\While\SingleWhileSingleElse.c++" },
            new object[] { @"Statements\While\SingleWhileBlockElse.c++" },
            new object[] { @"Statements\While\BlockWhileNoElse.c++" },
            new object[] { @"Statements\While\BlockWhileEmptyElse.c++" },
            new object[] { @"Statements\While\BlockWhileSingleElse.c++" },
            new object[] { @"Statements\While\BlockWhileBlockElse.c++" },
            new object[] { @"Statements\SwitchCase\NoCases.c++" },
            new object[] { @"Statements\SwitchCase\ManyCases.c++" },
            new object[] { @"Statements\WhileCase\NoCases.c++" },
            new object[] { @"Statements\WhileCase\ManyCases.c++" },
            new object[] { @"Statements\InCase\NoCases.c++" },
            new object[] { @"Statements\InCase\ManyCases.c++" },
            new object[] { @"Statements\ForCase\NoCases.c++" },
            new object[] { @"Statements\ForCase\ManyCases.c++" },
            new object[] { @"Statements\UntilCase\NoCases.c++" },
            new object[] { @"Statements\UntilCase\ManyCases.c++" },
            new object[] { @"Statements\Return\SimpleReturn.c++" },
            new object[] { @"Statements\Input\SimpleInput.c++" },
            new object[] { @"Statements\Print\SimplePrint.c++" },
            new object[] { @"Statements\Break\SimpleBreak.c++" },
            new object[] { @"Statements\Repeat\SimpleRepeat.c++" },
            new object[] { @"Statements\BlockAnywhere.c++" },
        ];
        private static readonly object[] sadTests =
        [
            new object[] { @"EmptyFile.c++", Is.EqualTo("Expected Program: Line 1 Column 1: EOF") },

            new object[] { @"Program\NoKeyword.c++", Is.EqualTo("Expected Program: Line 1 Column 1: Identifier \"progra\"") },
            new object[] { @"Program\NoID.c++", Is.EqualTo("Expected Program ID: Line 1 Column 9: Block Start") },
            new object[] { @"Program\NoBody.c++", Is.EqualTo("Expected Program Body: Line 1 Column 10: EOF") },
            new object[] { @"Program\NoBlockEnd.c++", Is.EqualTo("Expected Block End Token: Line 1 Column 17: EOF") },

            new object[] { @"Declarations\NoSemiColon.c++", Is.EqualTo("Expected Semi Colon: Line 4 Column 1: Block End") },
            new object[] { @"Declarations\NoSemiColonBetween.c++", Is.EqualTo("Expected Semi Colon: Line 4 Column 2: Declare") },
            new object[] { @"Declarations\NoCommaBetween.c++", Is.EqualTo("Expected Semi Colon: Line 3 Column 12: Identifier \"y\"") },

            new object[] { @"Functions\NoID.c++", Is.EqualTo("Expected Function ID: Line 3 Column 11: Parenthesis Start") },
            new object[] { @"Functions\NoParameters.c++", Is.EqualTo("Expected Formal Parameter List: Line 3 Column 13: Block Start") },
            new object[] { @"Functions\NoBody.c++", Is.EqualTo("Expected Function Body: Line 4 Column 1: Block End") },
            new object[] { @"Functions\Parameters\NoIDIn.c++", Is.EqualTo("Expected Parameter ID: Line 3 Column 15: Parenthesis End") },
            new object[] { @"Functions\Parameters\NoIDOut.c++", Is.EqualTo("Expected Parameter ID: Line 3 Column 16: Parenthesis End") },
            new object[] { @"Functions\Parameters\NoIDInOut.c++", Is.EqualTo("Expected Parameter ID: Line 3 Column 18: Parenthesis End") },
            new object[] { @"Functions\Parameters\OnlyComma.c++", Is.EqualTo("Expected Close Parenthesis Token: Line 3 Column 13: Comma") },
            new object[] { @"Functions\Parameters\TrailingComma.c++", Is.EqualTo("Expected Formal Parameter: Line 3 Column 15: Parenthesis End") },

            new object[] { @"Expressions\MultiplyFirst.c++", Is.EqualTo("Expected Expression: Line 3 Column 7: Multiply") },
            new object[] { @"Expressions\NoOperator.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 9: Constant 9") },
            new object[] { @"Expressions\DoubleOperator.c++", Is.EqualTo("Expected Term: Line 3 Column 9: Add") },
            new object[] { @"Expressions\ParenthesisAfterConstant.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 8: Parenthesis Start") },
            new object[] { @"Expressions\ConstantAfterIdentifier.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 9: Constant 9") },
            new object[] { @"Expressions\IdentifierAfterConstant.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 8: Identifier \"x\"") },
            new object[] { @"Expressions\NoCloseParenthesis.c++", Is.EqualTo("Expected Close Parenthesis Token: Line 3 Column 9: Semi Colon") },

            new object[] { @"Conditions\NoOperator.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 10: True") },
            new object[] { @"Conditions\DoubleOperator.c++", Is.EqualTo("Expected Bool Factor: Line 3 Column 14: And") }, // Not sure why it's "Term" for expression, but "Factor" for condition...
            new object[] { @"Conditions\BracketAfterConstant.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 9: Square Bracket Start") },
            new object[] { @"Conditions\BracketAfterComparison.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 8: Square Bracket Start") },
            new object[] { @"Conditions\ConstantAfterComparison.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 9: True") },
            new object[] { @"Conditions\ComparisonAfterConstant.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 10: Constant 9") },
            new object[] { @"Conditions\NoCloseBracket.c++", Is.EqualTo("Expected Close Square Bracket Token: Line 3 Column 11: Semi Colon") },

            new object[] { @"Statements\Assignment\NoID.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 2: Assignment") },
            new object[] { @"Statements\Assignment\NoAssignment.c++", Is.EqualTo("Expected Assignment Token: Line 3 Column 4: Constant 9") },
            new object[] { @"Statements\Assignment\NoExpression.c++", Is.EqualTo("Expected Expression: Line 4 Column 1: Block End") },

            new object[] { @"Statements\If\SemiColonBetween.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 16: Else") },
            new object[] { @"Statements\If\BlockSemiColonBetween.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 15: Else") },

            new object[] { @"Statements\While\SemiColonBetween.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 19: Else") },
            new object[] { @"Statements\While\BlockSemiColonBetween.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 18: Else") },

            new object[] { @"Statements\SwitchCase\NoDefault.c++", Is.EqualTo("Expected \"default\" Keyword: Line 5 Column 1: Block End") },
            new object[] { @"Statements\SwitchCase\NoDefaultColon.c++", Is.EqualTo("Expected Colon: Line 5 Column 10: Identifier \"x\"") },
            new object[] { @"Statements\SwitchCase\NoWhenColon.c++", Is.EqualTo("Expected Colon: Line 4 Column 13: Block Start") },
            new object[] { @"Statements\SwitchCase\NoCondition.c++", Is.EqualTo("Expected Condition: Line 4 Column 6: Case Start") },

            new object[] { @"Statements\WhileCase\NoDefault.c++", Is.EqualTo("Expected \"default\" Keyword: Line 5 Column 1: Block End") },
            new object[] { @"Statements\WhileCase\NoDefaultColon.c++", Is.EqualTo("Expected Colon: Line 5 Column 10: Identifier \"x\"") },
            new object[] { @"Statements\WhileCase\NoWhenColon.c++", Is.EqualTo("Expected Colon: Line 4 Column 13: Block Start") },
            new object[] { @"Statements\WhileCase\NoCondition.c++", Is.EqualTo("Expected Condition: Line 4 Column 6: Case Start") },

            new object[] { @"Statements\InCase\NoColon.c++", Is.EqualTo("Expected Colon: Line 4 Column 13: Block Start") },
            new object[] { @"Statements\InCase\NoCondition.c++", Is.EqualTo("Expected Condition: Line 4 Column 6: Case Start") },
            new object[] { @"Statements\InCase\SemiColonBetween.c++", Is.EqualTo("Expected Block End Token: Line 5 Column 2: When") },

            new object[] { @"Statements\ForCase\NoID.c++", Is.EqualTo("Expected Iteration Identifier: Line 3 Column 10: EqualTo") },
            new object[] { @"Statements\ForCase\NoEquals.c++", Is.EqualTo("Expected Equals Sign: Line 3 Column 12: Constant 1") },
            new object[] { @"Statements\ForCase\NoExpression.c++", Is.EqualTo("Expected Expression: Line 4 Column 2: When") },
            new object[] { @"Statements\ForCase\NoColon.c++", Is.EqualTo("Expected Colon: Line 4 Column 13: Block Start") },
            new object[] { @"Statements\ForCase\NoCondition.c++", Is.EqualTo("Expected Condition: Line 4 Column 6: Case Start") },
            new object[] { @"Statements\ForCase\SemiColonBetween.c++", Is.EqualTo("Expected Block End Token: Line 5 Column 2: When") },

            new object[] { @"Statements\UntilCase\NoUntil.c++", Is.EqualTo("Expected \"until\" Keyword: Line 5 Column 1: Block End") },
            new object[] { @"Statements\UntilCase\NoUntilCondition.c++", Is.EqualTo("Expected Condition: Line 6 Column 1: Block End") },
            new object[] { @"Statements\UntilCase\NoWhenCondition.c++", Is.EqualTo("Expected Condition: Line 4 Column 6: Case Start") },
            new object[] { @"Statements\UntilCase\NoWhenColon.c++", Is.EqualTo("Expected Colon: Line 4 Column 13: Block Start") },

            new object[] { @"Statements\Return\NoExpression.c++", Is.EqualTo("Expected Expression: Line 4 Column 1: Block End") },
            new object[] { @"Statements\Input\NoID.c++", Is.EqualTo("Expected Variable ID: Line 4 Column 1: Block End") },
            new object[] { @"Statements\Input\Expression.c++", Is.EqualTo("Expected Block End Token: Line 3 Column 10: Subtract") },
            new object[] { @"Statements\Print\NoExpression.c++", Is.EqualTo("Expected Expression: Line 4 Column 1: Block End") },
            new object[] { @"Statements\Break\NoCount.c++", Is.EqualTo("Expected Break Count: Line 4 Column 1: Block End") },
            new object[] { @"Statements\Repeat\NoIndex.c++", Is.EqualTo("Expected Repeat Index: Line 4 Column 1: Block End") },
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
            using BlockingCollection<Token> tokenQueue = [];

            lexicalAnalyser.Analyse(File.OpenRead(path), tokenQueue);
            syntaxAnalyser.Analyse(tokenQueue);
        }

        [TestCaseSource(nameof(sadTests))]
        public void SadTest(string file, NUnit.Framework.Constraints.IResolveConstraint messageConstraint)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"SyntaxAnalyserTests\Sad\{file}");

            var e = Assert.Throws<SyntaxAnalyserException>(() =>
            {
                using BlockingCollection<Token> tokenQueue = [];
                lexicalAnalyser.Analyse(File.OpenRead(path), tokenQueue);
                syntaxAnalyser.Analyse(tokenQueue);
            },
            $"Expected SyntaxAnalyserException");
            Assert.That(e.Message, messageConstraint);
        }
    }
}
