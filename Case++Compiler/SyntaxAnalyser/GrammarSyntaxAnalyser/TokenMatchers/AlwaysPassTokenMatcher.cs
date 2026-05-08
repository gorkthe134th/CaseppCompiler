using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    public class AlwaysPassTokenMatcher(string name) : TokenMatcher(name)
    {
        public override Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program) =>
            Task.FromResult<bool?>(true);
    }
}
