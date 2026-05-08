using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class KleeneStarTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override async Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program)
        {
            bool? match, matchSoFar = null;
            do matchSoFar |= match = await matcher.TryMatch(tokens, program); while (match == true);
            return matchSoFar;
        }
    }
}
