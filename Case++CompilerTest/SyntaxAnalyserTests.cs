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
            new object[] { @"ILInstructions\NoInstruction.c++" },
            new object[] { @"ILInstructions\Label.c++" },
            new object[] { @"ILInstructions\OnlyLabel.c++" },
            new object[] { @"ILInstructions\SimpleInstruction.c++" },
            new object[] { @"ILInstructions\Block.c++" },
        ];

        private static readonly object[] sadTests =
        [
            new object[] { @"EmptyFile.c++", Is.EqualTo("Line 1, Column 1: Expected Program, but got EOF.") },

            new object[] { @"Program\NoKeyword.c++", Is.EqualTo("Line 1, Column 1: Expected Program, but got Identifier \"progra\".") },
            new object[] { @"Program\NoID.c++", Is.EqualTo("Line 1, Column 9: Expected Program ID, but got Block Start.") },
            new object[] { @"Program\NoBody.c++", Is.EqualTo("Line 1, Column 10: Expected Program Body, but got EOF.") },
            new object[] { @"Program\NoBlockEnd.c++", Is.EqualTo("Line 1, Column 17: Expected Block End Token, but got EOF.") },

            new object[] { @"Declarations\NoSemiColon.c++", Is.EqualTo("Line 4, Column 1: Expected Semi Colon, but got Block End.") },
            new object[] { @"Declarations\NoSemiColonBetween.c++", Is.EqualTo("Line 4, Column 2: Expected Semi Colon, but got Declare.") },
            new object[] { @"Declarations\NoCommaBetween.c++", Is.EqualTo("Line 3, Column 12: Expected Semi Colon, but got Identifier \"y\".") },

            new object[] { @"Functions\NoID.c++", Is.EqualTo("Line 3, Column 11: Expected Function ID, but got Parenthesis Start.") },
            new object[] { @"Functions\NoParameters.c++", Is.EqualTo("Line 3, Column 13: Expected Formal Parameter List, but got Block Start.") },
            new object[] { @"Functions\NoBody.c++", Is.EqualTo("Line 4, Column 1: Expected Function Body, but got Block End.") },
            new object[] { @"Functions\Parameters\NoIDIn.c++", Is.EqualTo("Line 3, Column 15: Expected Parameter ID, but got Parenthesis End.") },
            new object[] { @"Functions\Parameters\NoIDOut.c++", Is.EqualTo("Line 3, Column 16: Expected Parameter ID, but got Parenthesis End.") },
            new object[] { @"Functions\Parameters\NoIDInOut.c++", Is.EqualTo("Line 3, Column 18: Expected Parameter ID, but got Parenthesis End.") },
            new object[] { @"Functions\Parameters\OnlyComma.c++", Is.EqualTo("Line 3, Column 13: Expected Close Parenthesis Token, but got Comma.") },
            new object[] { @"Functions\Parameters\TrailingComma.c++", Is.EqualTo("Line 3, Column 15: Expected Formal Parameter, but got Parenthesis End.") },

            new object[] { @"Expressions\MultiplyFirst.c++", Is.EqualTo("Line 3, Column 7: Expected Expression, but got Multiply.") },
            new object[] { @"Expressions\NoOperator.c++", Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Constant 9.") },
            new object[] { @"Expressions\DoubleOperator.c++", Is.EqualTo("Line 3, Column 9: Expected Term, but got Add.") },
            new object[] { @"Expressions\ParenthesisAfterConstant.c++", Is.EqualTo("Line 3, Column 8: Expected Block End Token, but got Parenthesis Start.") },
            new object[] { @"Expressions\ConstantAfterIdentifier.c++", Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Constant 9.") },
            new object[] { @"Expressions\IdentifierAfterConstant.c++", Is.EqualTo("Line 3, Column 8: Expected Block End Token, but got Identifier \"x\".") },
            new object[] { @"Expressions\NoCloseParenthesis.c++", Is.EqualTo("Line 3, Column 9: Expected Close Parenthesis Token, but got Semi Colon.") },

            new object[] { @"Conditions\NoOperator.c++", Is.EqualTo("Line 3, Column 10: Expected Block End Token, but got Constant True.") },
            new object[] { @"Conditions\DoubleOperator.c++", Is.EqualTo("Line 3, Column 14: Expected Bool Factor, but got And.") }, // Not sure why it's "Term" for expression, but "Factor" for condition...
            new object[] { @"Conditions\BracketAfterConstant.c++", Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Square Bracket Start.") },
            new object[] { @"Conditions\BracketAfterComparison.c++", Is.EqualTo("Line 3, Column 8: Expected Block End Token, but got Square Bracket Start.") },
            new object[] { @"Conditions\ConstantAfterComparison.c++", Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Constant True.") },
            new object[] { @"Conditions\ComparisonAfterConstant.c++", Is.EqualTo("Line 3, Column 10: Expected Block End Token, but got Constant 9.") },
            new object[] { @"Conditions\NoCloseBracket.c++", Is.EqualTo("Line 3, Column 11: Expected Close Square Bracket Token, but got Semi Colon.") },

            new object[] { @"Statements\Assignment\NoID.c++", Is.EqualTo("Line 3, Column 2: Expected Block End Token, but got Assignment Token.") },
            new object[] { @"Statements\Assignment\NoAssignment.c++", Is.EqualTo("Line 3, Column 4: Expected Assignment Token, but got Constant 9.") },
            new object[] { @"Statements\Assignment\NoExpression.c++", Is.EqualTo("Line 4, Column 1: Expected Expression, but got Block End.") },

            new object[] { @"Statements\If\SemiColonBetween.c++", Is.EqualTo("Line 3, Column 16: Expected Block End Token, but got Else.") },
            new object[] { @"Statements\If\BlockSemiColonBetween.c++", Is.EqualTo("Line 3, Column 15: Expected Block End Token, but got Else.") },

            new object[] { @"Statements\While\SemiColonBetween.c++", Is.EqualTo("Line 3, Column 19: Expected Block End Token, but got Else.") },
            new object[] { @"Statements\While\BlockSemiColonBetween.c++", Is.EqualTo("Line 3, Column 18: Expected Block End Token, but got Else.") },

            new object[] { @"Statements\SwitchCase\NoDefault.c++", Is.EqualTo("Line 5, Column 1: Expected \"default\" Keyword, but got Block End.") },
            new object[] { @"Statements\SwitchCase\NoDefaultColon.c++", Is.EqualTo("Line 5, Column 10: Expected Colon, but got Identifier \"x\".") },
            new object[] { @"Statements\SwitchCase\NoWhenColon.c++", Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") },
            new object[] { @"Statements\SwitchCase\NoCondition.c++", Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") },

            new object[] { @"Statements\WhileCase\NoDefault.c++", Is.EqualTo("Line 5, Column 1: Expected \"default\" Keyword, but got Block End.") },
            new object[] { @"Statements\WhileCase\NoDefaultColon.c++", Is.EqualTo("Line 5, Column 10: Expected Colon, but got Identifier \"x\".") },
            new object[] { @"Statements\WhileCase\NoWhenColon.c++", Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") },
            new object[] { @"Statements\WhileCase\NoCondition.c++", Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") },

            new object[] { @"Statements\InCase\NoColon.c++", Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") },
            new object[] { @"Statements\InCase\NoCondition.c++", Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") },
            new object[] { @"Statements\InCase\SemiColonBetween.c++", Is.EqualTo("Line 5, Column 2: Expected Block End Token, but got When.") },

            new object[] { @"Statements\ForCase\NoID.c++", Is.EqualTo("Line 3, Column 10: Expected Iteration Identifier, but got EqualTo.") },
            new object[] { @"Statements\ForCase\NoEquals.c++", Is.EqualTo("Line 3, Column 12: Expected Equals Sign, but got Constant 1.") },
            new object[] { @"Statements\ForCase\NoExpression.c++", Is.EqualTo("Line 4, Column 2: Expected Expression, but got When.") },
            new object[] { @"Statements\ForCase\NoColon.c++", Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") },
            new object[] { @"Statements\ForCase\NoCondition.c++", Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") },
            new object[] { @"Statements\ForCase\SemiColonBetween.c++", Is.EqualTo("Line 5, Column 2: Expected Block End Token, but got When.") },

            new object[] { @"Statements\UntilCase\NoUntil.c++", Is.EqualTo("Line 5, Column 1: Expected \"until\" Keyword, but got Block End.") },
            new object[] { @"Statements\UntilCase\NoUntilCondition.c++", Is.EqualTo("Line 6, Column 1: Expected Condition, but got Block End.") },
            new object[] { @"Statements\UntilCase\NoWhenCondition.c++", Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") },
            new object[] { @"Statements\UntilCase\NoWhenColon.c++", Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") },

            new object[] { @"Statements\Return\NoExpression.c++", Is.EqualTo("Line 4, Column 1: Expected Expression, but got Block End.") },
            new object[] { @"Statements\Input\NoID.c++", Is.EqualTo("Line 4, Column 1: Expected Variable ID, but got Block End.") },
            new object[] { @"Statements\Input\Expression.c++", Is.EqualTo("Line 3, Column 10: Expected Block End Token, but got Subtract.") },
            new object[] { @"Statements\Print\NoExpression.c++", Is.EqualTo("Line 4, Column 1: Expected Expression, but got Block End.") },
            new object[] { @"Statements\Break\NoCount.c++", Is.EqualTo("Line 4, Column 1: Expected Break Count, but got Block End.") },
            new object[] { @"Statements\Repeat\NoIndex.c++", Is.EqualTo("Line 4, Column 1: Expected Repeat Index, but got Block End.") },
            
            new object[] { @"ILInstructions\Expression.c++", Is.EqualTo("Line 3, Column 9: Expected Comma, but got Add.") },
            new object[] { @"ILInstructions\LessArguments.c++", Is.EqualTo("Line 3, Column 12: Expected Comma, but got Semi Colon.") },
            new object[] { @"ILInstructions\NoComma.c++", Is.EqualTo("Line 3, Column 7: Expected Comma, but got Constant 9.") },
            new object[] { @"ILInstructions\SemiColonInBlock.c++", Is.EqualTo("Line 4, Column 14: Expected Block End Token, but got Semi Colon.") },
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
