using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    public class AlwaysPassTokenMatcher(string name) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens) => true;
    }
}
