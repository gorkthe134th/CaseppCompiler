using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class SemiColonTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return static c => c == ';';
            }
        }

        public override int Limit => 1;

        public override Token GenerateToken(string text, int line, int column) =>
            new SemiColonToken(line, column);
    }
}
