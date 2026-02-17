using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class EmptyTokenMatcher(string name) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => true;

        public override bool CanMatch(Token firstToken) => false;

        public override void Match(IEnumerator<Token> tokens) { }
    }
}
