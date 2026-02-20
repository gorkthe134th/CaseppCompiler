using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SequenceTokenMatcher(string name, TokenMatcher[] matchers) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            bool? matchSoFar = null;
            foreach (var matcher in matchers)
            {
                bool? match = matcher.TryMatch(tokens);
                if (match == false)
                    return matchSoFar == true
                        ? throw new ArgumentException($"Expected {matcher.Name}: {tokens.Current}")
                        : false;
                matchSoFar |= match;
            }
            return matchSoFar;
        }
    }
}
