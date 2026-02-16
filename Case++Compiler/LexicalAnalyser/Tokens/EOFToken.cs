using System;
using System.Collections.Generic;
using System.Text;

namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class EOFToken(int line, int column) : Token(line, column)
    {
        public override string ToString() => $"{base.ToString()} EOF";
    }
}
