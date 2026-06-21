using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class CodeTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return c => c == '$';
                yield return c => c == '{';
                bool ending = false;
                bool end = false;
                while (!end)
                {
                    yield return ending
                        ? (c => { ending = c == '$'; end = c == '}'; return true; })
                        : (c => { ending = c == '$'; return true; });
                }
            }
        }

        public override int Limit => int.MaxValue;

        public override Token? GenerateToken(Position position, string text) =>
            new CodeToken(position, text.AsMemory().Slice(2, text.Length - 4));
    }
}
