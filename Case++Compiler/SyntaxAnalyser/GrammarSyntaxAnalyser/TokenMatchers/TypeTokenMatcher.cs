using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class TypeTokenMatcher<T>(string name) : TokenMatcher(name) where T : Token
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            if (tokens.Current is not T) return false;

            if (!tokens.MoveNext() && !typeof(T).IsAssignableTo(typeof(EOFToken)))
                throw new ArgumentException($"Expected EOF Token");

            return true;
        }
    }
}
