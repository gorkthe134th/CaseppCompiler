using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class UnderscoreTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return static c => c == '_';
            }
        }

        public override int Limit => 1;

        public override Token GenerateToken(Position position, string text) =>
            new UnderscoreToken(position);
    }
}
