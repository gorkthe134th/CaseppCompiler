using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class MultiLineCommentTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return c => c == '/';
                yield return c => c == '*';
                bool ending = false;
                bool end = false;
                while (!end)
                {
                    yield return ending
                        ? (c => { ending = c == '*'; end = c == '/'; return true; })
                        : (c => { ending = c == '*'; return true; });
                }
            }
        }

        public override int Limit => 2;

        public override Token? GenerateToken(string text, int line, int column) => null;
    }
}
