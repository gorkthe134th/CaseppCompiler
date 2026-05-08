using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class AlternativeTokenMatcher(string name, TokenMatcher[] matchers) : TokenMatcher(name)
    {
        public override async Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program)
        {
            bool? matchSoFar = false;
            foreach (var matcher in matchers)
            {
                bool? match = await matcher.TryMatch(tokens, program);
                if (match == true) return true;
                matchSoFar |= match;
            }
            return matchSoFar;
        }
    }
}
