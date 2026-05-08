using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class OperatorTokenMatcher(string name, OperationType operation) : TokenMatcher(name)
    {
        public override async Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program)
        {
            if (tokens.Current is not OperatorToken operatorToken ||
                operatorToken.Operation != operation) return false;
            program?.PushCompilerVariable(operatorToken.Operation);
            await MoveNext(tokens);

            return true;
        }
    }
}
