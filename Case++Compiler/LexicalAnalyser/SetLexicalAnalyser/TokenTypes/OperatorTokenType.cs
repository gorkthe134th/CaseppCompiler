using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class OperatorTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                ICollection<char> additionalAllowedChapacters = [];
                yield return c =>
                {
                    switch (c)
                    {
                        case '+' or '-' or '*' or '/' or '=':
                            return true;
                        case '<':
                            additionalAllowedChapacters = ['=', '>'];
                            return true;
                        case '>':
                            additionalAllowedChapacters = ['='];
                            return true;
                        default:
                            return false;
                    };
                };
                if (additionalAllowedChapacters.Count > 0) yield return c => additionalAllowedChapacters.Contains(c);
            }
        }

        public override int Limit => 2;

        public override Token GenerateToken(string text, int line, int column) =>
            new OperatorToken(text, line, column);
    }
}
