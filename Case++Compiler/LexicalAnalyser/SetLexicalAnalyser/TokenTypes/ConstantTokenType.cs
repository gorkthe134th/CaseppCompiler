using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class ConstantTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                while (true) yield return static c => ('0' <= c && c <= '9') ? null : false;
            }
        }

        public override int Limit => 6;

        public override Token GenerateToken(Position position, string text) =>
            new ConstantToken(position, uint.Parse(text));
    }
}
