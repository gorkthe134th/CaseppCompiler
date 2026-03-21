using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SequenceTokenMatcher(string name, TokenMatcher[] matchers) : TokenMatcher(name)
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
        {
            bool? matchSoFar = null;
            foreach (var matcher in matchers)
            {
                bool? match = matcher.TryMatch(tokens, program);
                if (match == false)
                    return matchSoFar == true
                        ? throw new SyntaxAnalyserException($"Expected {matcher.Name}: {tokens.Current}")
                        : false;
                matchSoFar |= match;
            }
            return matchSoFar;
        }
    }
}
