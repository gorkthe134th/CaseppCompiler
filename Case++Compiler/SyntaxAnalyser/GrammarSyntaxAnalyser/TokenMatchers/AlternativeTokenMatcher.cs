using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class AlternativeTokenMatcher(string name, TokenMatcher[] matcherOptions) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => matcherOptions.Any(m => m.CanMatchEmpty);

        public override bool CanMatch(Token firstToken) => matcherOptions.Any(m => m.CanMatch(firstToken));

        public override void Match(IEnumerator<Token> tokens)
        {
            foreach (var matcher in matcherOptions)
            {
                if (matcher.CanMatch(tokens.Current))
                {
                    matcher.Match(tokens);
                    return;
                }
            }
            throw new ArgumentException($"Expected {Name}: {tokens.Current}");
        }
    }
}
