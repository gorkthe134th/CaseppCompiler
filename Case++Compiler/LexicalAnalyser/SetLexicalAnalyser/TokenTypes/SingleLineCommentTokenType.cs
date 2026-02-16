using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class SingleLineCommentTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return static c => c == '/';
                yield return static c => c == '/';
                bool end = false;
                while (!end) yield return c => { end = c == '\n'; return null; };
            }
        }

        public override int Limit => 0;

        public override Token? GenerateToken(string text, int line, int column) => null;
    }
}
