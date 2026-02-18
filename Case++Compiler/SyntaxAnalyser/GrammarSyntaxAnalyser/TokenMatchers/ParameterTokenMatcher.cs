using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class ParameterTokenMatcher(string name, TokenMatcher contentMatcher) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => false;

        public override bool CanMatch(Token firstToken) =>
            firstToken is ParameterListToken startToken &&
            startToken.Type == RegionMarkType.Start;

        public override void Match(IEnumerator<Token> tokens)
        {
            if (!tokens.MoveNext()) throw new ArgumentException($"Expected {Name}");

            if (contentMatcher.CanMatch(tokens.Current)) contentMatcher.Match(tokens);

            if (tokens.Current is not ParameterListToken endToken ||
                endToken.Type != RegionMarkType.End) throw new ArgumentException($"Expected Block End Token: {tokens.Current}");

            if (!tokens.MoveNext()) throw new ArgumentException($"Expected EOF Token");
        }
    }
}
