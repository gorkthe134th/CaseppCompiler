using CaseppCompiler.LexicalAnalyser.Tokens;

using System;
using System.Collections.Generic;
using System.Text;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class KleeneStarTokenMatcher(string name, TokenMatcher matcher) : TokenMatcher(name)
    {
        public override bool? TryMatch(IEnumerator<Token> tokens)
        {
            bool? match = null;
            while ((match |= matcher.TryMatch(tokens)) == true) ;
            return match;
        }
    }
}
