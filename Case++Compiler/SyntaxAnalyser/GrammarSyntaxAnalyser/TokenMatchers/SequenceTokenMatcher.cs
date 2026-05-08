using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SequenceTokenMatcher(string name, TokenMatcher[] matchers) : TokenMatcher(name)
    {
        public override async Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program)
        {
            bool? matchSoFar = null;
            foreach (var matcher in matchers)
            {
                bool? match = await matcher.TryMatch(tokens, program);
                if (match == false)
                    return matchSoFar == true
                        ? throw new SyntaxAnalyserException(tokens.Current.Position, $"Expected {matcher.Name}, but got {tokens.Current}.")
                        : false;
                matchSoFar |= match;
            }
            return matchSoFar;
        }
    }
}
