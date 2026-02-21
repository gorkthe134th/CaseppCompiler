using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class ParenthesesTokenMatcher(string name, TokenMatcher contentMatcher) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            if (tokens.Current is not ParenthesisToken startToken ||
                startToken.Type != RegionMarkType.Start) return false;
            MoveNext(tokens);

            if (contentMatcher.TryMatch(tokens) == false) throw new SyntaxAnalyserException($"Expected {Name}: {tokens.Current}");

            if (tokens.Current is not ParenthesisToken endToken ||
                endToken.Type != RegionMarkType.End) throw new SyntaxAnalyserException($"Expected Close Parenthesis Token: {tokens.Current}");
            MoveNext(tokens);

            return true;
        }
    }
}
