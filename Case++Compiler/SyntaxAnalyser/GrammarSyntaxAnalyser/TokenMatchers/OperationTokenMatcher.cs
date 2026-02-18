using CaseppCompiler.LexicalAnalyser.Tokens;

using System;
using System.Collections.Generic;
using System.Text;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class OperatorTokenMatcher(string name, OperatorToken.OperationType operation) : TokenMatcher(name)
    {
        public override bool CanMatchEmpty => false;

        public override bool CanMatch(Token firstToken) =>
            firstToken is OperatorToken operatorToken &&
            operatorToken.Operation == operation;

        public override void Match(IEnumerator<Token> tokens)
        {
            if (!tokens.MoveNext()) throw new ArgumentException($"Expected EOF Token");
        }
    }
}
