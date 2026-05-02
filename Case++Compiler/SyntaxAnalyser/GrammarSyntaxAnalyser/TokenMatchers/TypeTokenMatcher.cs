using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class TypeTokenMatcher<T>(string name) : TokenMatcher(name) where T : Token
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
        {
            if (tokens.Current is not T) return false;

            if (program != null)
            {
                if (tokens.Current is IdentifierToken identifier) program.PushCompilerVariable(identifier.Name);
                if (tokens.Current is ConstantToken     constant) program.PushCompilerVariable(constant.Constant);
                if (tokens.Current is BoolConstantToken    @bool) program.PushCompilerVariable(@bool.Constant);
                if (tokens.Current is OperatorToken    @operator) program.PushCompilerVariable(@operator.Operation);
            }

            Position currentPosition = tokens.Current.Position;
            if (!tokens.MoveNext() && !typeof(T).IsAssignableTo(typeof(EOFToken)))
                throw new SyntaxAnalyserException(currentPosition + 1, $"Expected EOF Token.");

            return true;
        }
    }
}
