using CaseppCompiler.LexicalAnalyser.Tokens;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.TokenTypes
{
    internal partial class IdentifierTokenType(OperatorTokenType operatorTokenType) : TokenType
    {
        [GeneratedRegex("^[A-Za-z0-9]{1,30}")]
        public override partial Regex Regex();

        public override Token GenerateToken(string text, int line, int column) =>
            operatorTokenType.Regex().Match(text).Length == text.Length
                ? new OperatorToken(text, line, column)
                : new IdentifierToken(text, line, column);
    }
}
