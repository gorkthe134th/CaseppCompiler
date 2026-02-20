using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class OperatorTokenMatcher(string name, OperatorToken.OperationType operation) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            if (tokens.Current is not OperatorToken operatorToken ||
                operatorToken.Operation != operation) return false;
            MoveNext(tokens);

            return true;
        }
    }
}
