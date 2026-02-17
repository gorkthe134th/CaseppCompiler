using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Diagnostics;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SequenceTokenMatcher(string name, IEnumerable<TokenMatcher> matcherSequence) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => matcherSequence.All(m => m.CanMatchEmpty);

        public override bool CanMatch(Token firstToken)
        {
            var matcherEnumerator = matcherSequence.GetEnumerator();
            while (matcherEnumerator.MoveNext())
            {
                if (matcherEnumerator.Current.CanMatch(firstToken)) return true;
                if (!matcherEnumerator.Current.CanMatchEmpty) return false;
            }
            return false;
        }

        public override void Match(IEnumerator<Token> tokens)
        {
            var matcherEnumerator = matcherSequence.GetEnumerator();

            if (!matcherEnumerator.MoveNext()) throw new UnreachableException("Matcher Sequence is empty");
            matcherEnumerator.Current.Match(tokens);

            while (matcherEnumerator.MoveNext())
            {
                if (!tokens.MoveNext())
                {
                    do
                        if (!matcherEnumerator.Current.CanMatchEmpty)
                            throw new ArgumentException($"Expected {matcherEnumerator.Current.Name}");
                    while (matcherEnumerator.MoveNext());
                    return;
                }
                var matcher = matcherEnumerator.Current;
                if (matcher.CanMatch(tokens.Current))
                {
                    matcher.Match(tokens);
                    continue;
                }
                if (matcher.CanMatchEmpty)
                    continue;
                throw new ArgumentException($"Expected {matcher.Name}: {tokens.Current}");
            }
        }
    }
}
