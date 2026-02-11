using CaseppCompiler.LexicalAnalyser.Tokens;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.TokenTypes
{
    internal partial class OperatorTokenType : TokenType
    {
        [GeneratedRegex(@"^(not)|\+|-|\*|/|=|<|>|(<>)|(<=)|(>=)|(and)|(or)")]
        public override partial Regex Regex();

        public override Token GenerateToken(string text, int line, int column) =>
            new OperatorToken(text, line, column);
    }
}
