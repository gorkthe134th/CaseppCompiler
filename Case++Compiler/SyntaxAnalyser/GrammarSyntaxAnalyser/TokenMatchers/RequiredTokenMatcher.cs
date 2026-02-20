using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    public class RequiredTokenMatcher(string name) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens) => true;
    }
}
