using CaseppCompiler.CodeGenerator;
using CaseppCompiler.LexicalAnalyser;
using CaseppCompiler.SyntaxAnalyser;

using NUnit.Framework.Constraints;

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
            new object[] { @"EmptyFile.c++", new IResolveConstraint[] { Is.EqualTo("Line 1, Column 1: Expected Program, but got EOF.") } },

            new object[] { @"Program\NoKeyword.c++", new IResolveConstraint[] { Is.EqualTo("Line 1, Column 1: Expected Program, but got Identifier \"progra\".") } },
            new object[] { @"Program\NoID.c++", new IResolveConstraint[] { Is.EqualTo("Line 1, Column 9: Expected Program ID, but got Block Start.") } },
            new object[] { @"Program\NoBody.c++", new IResolveConstraint[] { Is.EqualTo("Line 1, Column 10: Expected Program Body, but got EOF.") } },
            new object[] { @"Program\NoBlockEnd.c++", new IResolveConstraint[] { Is.EqualTo("Line 1, Column 17: Expected Block End Token, but got EOF.") } },

            new object[] { @"Declarations\NoSemiColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Semi Colon, but got Block End.") } },
            new object[] { @"Declarations\NoSemiColonBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 2: Expected Semi Colon, but got Declare.") } },
            new object[] { @"Declarations\NoCommaBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 12: Expected Semi Colon, but got Identifier \"y\".") } },

            new object[] { @"Functions\NoID.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 11: Expected Function ID, but got Parenthesis Start.") } },
            new object[] { @"Functions\NoParameters.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 13: Expected Formal Parameter List, but got Block Start.") } },
            new object[] { @"Functions\NoBody.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Function Body, but got Block End.") } },
            new object[] { @"Functions\Parameters\NoIDIn.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 15: Expected Parameter ID, but got Parenthesis End.") } },
            new object[] { @"Functions\Parameters\NoIDOut.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 16: Expected Parameter ID, but got Parenthesis End.") } },
            new object[] { @"Functions\Parameters\NoIDInOut.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 18: Expected Parameter ID, but got Parenthesis End.") } },
            new object[] { @"Functions\Parameters\OnlyComma.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 13: Expected Close Parenthesis Token, but got Comma.") } },
            new object[] { @"Functions\Parameters\TrailingComma.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 15: Expected Formal Parameter, but got Parenthesis End.") } },

            new object[] { @"Expressions\MultiplyFirst.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 7: Expected Expression, but got Multiply.") } },
            new object[] { @"Expressions\NoOperator.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Constant 9.") } },
            new object[] { @"Expressions\DoubleOperator.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 9: Expected Term, but got Add.") } },
            new object[] { @"Expressions\ParenthesisAfterConstant.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 8: Expected Block End Token, but got Parenthesis Start.") } },
            new object[] { @"Expressions\ConstantAfterIdentifier.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Constant 9.") } },
            new object[] { @"Expressions\IdentifierAfterConstant.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 8: Expected Block End Token, but got Identifier \"x\".") } },
            new object[] { @"Expressions\NoCloseParenthesis.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 9: Expected Close Parenthesis Token, but got Semi Colon.") } },

            new object[] { @"Conditions\NoOperator.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 10: Expected Block End Token, but got Constant True.") } },
            new object[] { @"Conditions\DoubleOperator.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 14: Expected Bool Factor, but got And.") } }, // Not sure why it's "Term" for expression, but "Factor" for condition...
            new object[] { @"Conditions\BracketAfterConstant.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Square Bracket Start.") } },
            new object[] { @"Conditions\BracketAfterComparison.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 8: Expected Block End Token, but got Square Bracket Start.") } },
            new object[] { @"Conditions\ConstantAfterComparison.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 9: Expected Block End Token, but got Constant True.") } },
            new object[] { @"Conditions\ComparisonAfterConstant.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 10: Expected Block End Token, but got Constant 9.") } },
            new object[] { @"Conditions\NoCloseBracket.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 11: Expected Close Square Bracket Token, but got Semi Colon.") } },

            new object[] { @"Statements\Assignment\NoID.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 2: Expected Block End Token, but got Assignment Token.") } },
            new object[] { @"Statements\Assignment\NoAssignment.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 4: Expected Assignment Token, but got Constant 9.") } },
            new object[] { @"Statements\Assignment\NoExpression.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Expression, but got Block End.") } },

            new object[] { @"Statements\If\SemiColonBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 16: Expected Block End Token, but got Else.") } },
            new object[] { @"Statements\If\BlockSemiColonBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 15: Expected Block End Token, but got Else.") } },

            new object[] { @"Statements\While\SemiColonBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 19: Expected Block End Token, but got Else.") } },
            new object[] { @"Statements\While\BlockSemiColonBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 18: Expected Block End Token, but got Else.") } },

            new object[] { @"Statements\SwitchCase\NoDefault.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 1: Expected \"default\" Keyword, but got Block End.") } },
            new object[] { @"Statements\SwitchCase\NoDefaultColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 10: Expected Colon, but got Identifier \"x\".") } },
            new object[] { @"Statements\SwitchCase\NoWhenColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") } },
            new object[] { @"Statements\SwitchCase\NoCondition.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") } },

            new object[] { @"Statements\WhileCase\NoDefault.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 1: Expected \"default\" Keyword, but got Block End.") } },
            new object[] { @"Statements\WhileCase\NoDefaultColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 10: Expected Colon, but got Identifier \"x\".") } },
            new object[] { @"Statements\WhileCase\NoWhenColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") } },
            new object[] { @"Statements\WhileCase\NoCondition.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") } },

            new object[] { @"Statements\InCase\NoColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") } },
            new object[] { @"Statements\InCase\NoCondition.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") } },
            new object[] { @"Statements\InCase\SemiColonBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 2: Expected Block End Token, but got When.") } },

            new object[] { @"Statements\ForCase\NoID.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 10: Expected Iteration Identifier, but got EqualTo.") } },
            new object[] { @"Statements\ForCase\NoEquals.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 12: Expected Equals Sign, but got Constant 1.") } },
            new object[] { @"Statements\ForCase\NoExpression.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 2: Expected Expression, but got When.") } },
            new object[] { @"Statements\ForCase\NoColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") } },
            new object[] { @"Statements\ForCase\NoCondition.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") } },
            new object[] { @"Statements\ForCase\SemiColonBetween.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 2: Expected Block End Token, but got When.") } },

            new object[] { @"Statements\UntilCase\NoUntil.c++", new IResolveConstraint[] { Is.EqualTo("Line 5, Column 1: Expected \"until\" Keyword, but got Block End.") } },
            new object[] { @"Statements\UntilCase\NoUntilCondition.c++", new IResolveConstraint[] { Is.EqualTo("Line 6, Column 1: Expected Condition, but got Block End.") } },
            new object[] { @"Statements\UntilCase\NoWhenCondition.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 6: Expected Condition, but got Colon.") } },
            new object[] { @"Statements\UntilCase\NoWhenColon.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 13: Expected Colon, but got Block Start.") } },

            new object[] { @"Statements\Return\NoExpression.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Expression, but got Block End.") } },
            new object[] { @"Statements\Input\NoID.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Variable ID, but got Block End.") } },
            new object[] { @"Statements\Input\Expression.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 10: Expected Block End Token, but got Subtract.") } },
            new object[] { @"Statements\Print\NoExpression.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Expression, but got Block End.") } },
            new object[] { @"Statements\Break\NoCount.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Break Count, but got Block End.") } },
            new object[] { @"Statements\Repeat\NoIndex.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 1: Expected Repeat Index, but got Block End.") } },
            
            new object[] { @"ILInstructions\Expression.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 9: Expected Comma, but got Add.") } },
            new object[] { @"ILInstructions\LessArguments.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 12: Expected Comma, but got Semi Colon.") } },
            new object[] { @"ILInstructions\NoComma.c++", new IResolveConstraint[] { Is.EqualTo("Line 3, Column 7: Expected Comma, but got Constant 9.") } },
            new object[] { @"ILInstructions\SemiColonInBlock.c++", new IResolveConstraint[] { Is.EqualTo("Line 4, Column 14: Expected Block End Token, but got Semi Colon.") } },
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

            using TokenStream tokens = new();

            lexicalAnalyser.Analyse(File.OpenRead(path), tokens);
            syntaxAnalyser.Analyse(tokens);
        }

        [TestCaseSource(nameof(sadTests))]
        public void SadTest(string file, IEnumerable<IResolveConstraint> messageConstraints)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $@"SyntaxAnalyserTests\Sad\{file}");

            Exception? e = Assert.Throws<SyntaxAnalyserException>(() =>
            {
                using TokenStream tokenQueue = new();
                lexicalAnalyser.Analyse(File.OpenRead(path), tokenQueue);
                syntaxAnalyser.Analyse(tokenQueue);
            },
            $"Expected SyntaxAnalyserException.");

            foreach (var messageConstraint in messageConstraints)
            {
                Assert.That(e, Is.Not.Null, $"Expected more Inner Exceptions.");
                Assert.That(e.Message, messageConstraint);
                e = e.InnerException;
            }
        }
    }
}
