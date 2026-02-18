using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class TypeTokenMatcher<T>(string name) : TokenMatcher(name) where T : Token
    {
        public override bool CanMatchEmpty => false;

        public override bool CanMatch(Token firstToken) => firstToken is T;

        public override void Match(IEnumerator<Token> tokens)
        {
            if (!tokens.MoveNext() && !typeof(T).IsSubclassOf(typeof(EOFToken)))
                throw new ArgumentException($"Expected EOF Token");
        }
    }
}
