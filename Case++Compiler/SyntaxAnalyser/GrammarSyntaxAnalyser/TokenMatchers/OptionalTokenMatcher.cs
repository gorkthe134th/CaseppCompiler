using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class OptionalTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens) => matcher.TryMatch(tokens) | null;
    }
}
