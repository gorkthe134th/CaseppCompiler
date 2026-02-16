using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal partial class ParameterListTokenType : TokenType
    {
        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return static c => c == '(' | c == ')';
            }
        }

        public override int Limit => 1;

        public override Token GenerateToken(string text, int line, int column) =>
            new ParameterListToken(
                text switch
                {
                    "(" => RegionMarkType.Start,
                    ")" => RegionMarkType.End,
                    _   => throw new ArgumentException($"Line {line} Column {column}: Invalid Parameter Mark \"{text}\"")
                },
                line, column);
    }
}
