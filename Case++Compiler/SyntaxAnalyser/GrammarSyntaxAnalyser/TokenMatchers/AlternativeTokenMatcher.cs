using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class AlternativeTokenMatcher(string name, TokenMatcher[] matchers) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            bool? matchSoFar = null;
            foreach (var matcher in matchers)
            {
                bool? match = matcher.TryMatch(tokens);
                if (match == true) return true;
                matchSoFar |= match;
            }
            return matchSoFar;
        }
    }
}
