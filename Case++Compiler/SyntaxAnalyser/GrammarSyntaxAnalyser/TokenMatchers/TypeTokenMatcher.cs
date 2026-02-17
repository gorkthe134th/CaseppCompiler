using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class TypeTokenMatcher<Type>(string name) : TokenMatcher(name) where Type : Token
    {
        public override bool CanMatchEmpty => false;

        public override bool CanMatch(Token firstToken) => firstToken is Type;

        public override void Match(IEnumerator<Token> tokens) { }
    }
}
