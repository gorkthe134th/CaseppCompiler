using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class KleeneStarTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
        {
            bool? match, matchSoFar = null;
            do matchSoFar |= match = matcher.TryMatch(tokens, program); while (match == true);
            return matchSoFar;
        }
    }
}
