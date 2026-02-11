using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Text;

namespace CaseppCompiler.LexicalAnalyser.Tokens
{
    internal class IdentifierToken(string name, int line, int column) : Token(line, column)
    {
        public string Name { get; } = name;

        public override string ToString() => $"{base.ToString()} Identifier \"{Name}\"";
    }
}
