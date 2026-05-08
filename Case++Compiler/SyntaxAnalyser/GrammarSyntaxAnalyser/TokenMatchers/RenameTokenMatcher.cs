using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class RenameTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program) =>
            matcher.TryMatch(tokens, program);
    }
}
