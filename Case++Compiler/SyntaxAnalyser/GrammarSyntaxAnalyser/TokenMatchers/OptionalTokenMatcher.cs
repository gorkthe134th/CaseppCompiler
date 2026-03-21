using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class OptionalTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program) =>
            matcher.TryMatch(tokens, program) | null;
    }
}
