using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    public class ImpossibleTokenMatcher(string name) : TokenMatcher(name)
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program) => false;
    }
}
