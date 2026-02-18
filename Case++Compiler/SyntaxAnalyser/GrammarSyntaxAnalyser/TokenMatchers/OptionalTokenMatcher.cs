using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class OptionalTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => true;

        public override bool CanMatch(Token firstToken) => matcher.CanMatch(firstToken);

        public override void Match(IEnumerator<Token> tokens) => matcher.Match(tokens);
    }
}
