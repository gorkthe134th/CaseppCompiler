using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SquareBracketTokenMatcher(string name, TokenMatcher contentMatcher) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            if (tokens.Current is not SquareBracketToken startToken ||
                startToken.Type != RegionMarkType.Start) return false;
            MoveNext(tokens);

            if (contentMatcher.TryMatch(tokens) == false) throw new ArgumentException($"Expected {Name}: {tokens.Current}");

            if (tokens.Current is not SquareBracketToken endToken ||
                endToken.Type != RegionMarkType.End) throw new ArgumentException($"Expected Close Parenthesis Token: {tokens.Current}");
            MoveNext(tokens);

            return true;
        }
    }
}
