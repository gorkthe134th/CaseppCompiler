using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Diagnostics;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SequenceTokenMatcher(string name, TokenMatcher[] matchers) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => matchers.All(m => m.CanMatchEmpty);

        public override bool CanMatch(Token firstToken)
        {
            foreach (var matcher in matchers)
            {
                if (matcher.CanMatch(firstToken)) return true;
                if (!matcher.CanMatchEmpty) return false;
            }
            return false;
        }

        public override void Match(IEnumerator<Token> tokens)
        {
            foreach (var matcher in matchers)
            {
                if (matcher.CanMatch(tokens.Current))
                {
                    matcher.Match(tokens);
                    continue;
                }
                if (!matcher.CanMatchEmpty) throw new ArgumentException($"Expected {matcher.Name}: {tokens.Current}");
            }
        }
    }
}
