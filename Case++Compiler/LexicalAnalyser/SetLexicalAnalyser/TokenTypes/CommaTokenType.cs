using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class CommaTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return static c => c == ',';
            }
        }

        public override int Limit => 1;

        public override Token GenerateToken(Position position, string text) =>
            new CommaToken(position);
    }
}
