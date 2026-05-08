using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    public class ImpossibleTokenMatcher(string name) : TokenMatcher(name)
    {
        public override Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program) =>
            Task.FromResult<bool?>(false);
    }
}
