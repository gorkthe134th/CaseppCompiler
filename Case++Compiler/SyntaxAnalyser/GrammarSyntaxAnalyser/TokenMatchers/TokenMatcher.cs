using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal abstract class TokenMatcher(string name)
    {
        public string Name { get; } = name;

        public abstract bool CanMatchEmpty { get; }

        public abstract bool CanMatch(Token firstToken);

        public abstract void Match(IEnumerator<Token> tokens);
    }
}
