using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class OperatorTokenMatcher(string name, OperatorToken.OperationType operation) : TokenMatcher(name)
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
        {
            if (tokens.Current is not OperatorToken operatorToken ||
                operatorToken.Operation != operation) return false;
            program?.PushVariable(operatorToken.Operation);
            MoveNext(tokens);

            return true;
        }
    }
}
