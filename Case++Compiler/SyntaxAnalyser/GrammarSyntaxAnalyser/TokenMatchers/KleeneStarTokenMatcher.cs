using CaseppCompiler.LexicalAnalyser.Tokens;

using System;
using System.Collections.Generic;
using System.Text;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class KleeneStarTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => true;

        public override bool CanMatch(Token firstToken) => matcher.CanMatch(firstToken);

        public override void Match(IEnumerator<Token> tokens)
        {
            while (matcher.CanMatch(tokens.Current)) matcher.Match(tokens);
        }
    }
}
