using CaseppCompiler.Tokens;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace CaseppCompiler
{
    internal class LexicalAnalyser
    {
        private static TokenType[] tokenTypes =
        [
            new ConstantTokenType()
        ];

        public IEnumerable<Token> Analyse(Stream input)
        {
            InputStream inputStream = new(input);
            int line = 1;
            while (!inputStream.EndOfStream)
            {
                inputStream.Trim(out int lineChanges);
                line += lineChanges;
                TokenType? matchedType = null;
                if (inputStream.TryMatchFirst(tokenTypes.Select(type => { matchedType = type; return type.Regex(); }), out string text))
                    yield return matchedType.GenerateToken(text, line);
                else
                    throw new ArgumentException($"Line {line}: Invalid Token");
            }
        }
    }
}
