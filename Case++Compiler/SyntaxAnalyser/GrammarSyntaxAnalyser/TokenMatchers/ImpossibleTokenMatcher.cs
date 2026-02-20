using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    public class ImpossibleTokenMatcher(string name) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens) => false;
    }
}
