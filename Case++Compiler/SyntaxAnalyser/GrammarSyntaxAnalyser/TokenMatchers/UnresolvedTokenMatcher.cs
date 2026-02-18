using CaseppCompiler.LexicalAnalyser.Tokens;

using System;
using System.Collections.Generic;
using System.Text;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class UnresolvedTokenMatcher(string temporaryName) : TokenMatcher(temporaryName)
    {
        TokenMatcher? matcher = null;

        private Exception Exception => new InvalidOperationException($"Matcher \"{Name}\" is Unresolved");

        public override string Name
        {
            get => matcher == null ? base.Name : matcher.Name;
            set
            {
                if (matcher == null) base.Name = value;
                else matcher.Name = value;
            }
        }

        public override bool CanMatchEmpty => matcher == null ? throw Exception : matcher.CanMatchEmpty;

        public override bool CanMatch(Token firstToken) => matcher == null ? throw Exception : matcher.CanMatch(firstToken);

        public override void Match(IEnumerator<Token> tokens)
        {
            if (matcher == null) throw Exception;
            matcher.Match(tokens);
        }

        public void Resolve(TokenMatcher matcher) => this.matcher = matcher;
    }
}
