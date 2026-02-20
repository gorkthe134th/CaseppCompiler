using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class KleeneStarTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            bool? match, matchSoFar = null;
            do matchSoFar |= match = matcher.TryMatch(tokens); while (match == true);
            return matchSoFar;
        }
    }
}
