using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class AssignmentTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return static c => c == ':';
                yield return static c => c == '=';
            }
        }

        public override int Limit => 2;

        public override Token GenerateToken(Position position, string text) =>
            new AssignmentToken(position);
    }
}
