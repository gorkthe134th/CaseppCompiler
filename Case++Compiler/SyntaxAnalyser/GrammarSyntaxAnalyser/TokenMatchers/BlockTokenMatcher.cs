using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class BlockTokenMatcher(string name, TokenMatcher contentMatcher) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            if (tokens.Current is not BlockToken startToken ||
                startToken.Type != RegionMarkType.Start) return false;
            MoveNext(tokens);

            if (contentMatcher.TryMatch(tokens) == false) throw new SyntaxAnalyserException($"Expected {Name}: {tokens.Current}");

            if (tokens.Current is not BlockToken endToken ||
                endToken.Type != RegionMarkType.End) throw new SyntaxAnalyserException($"Expected Block End Token: {tokens.Current}");
            MoveNext(tokens);

            return true;
        }
    }
}
