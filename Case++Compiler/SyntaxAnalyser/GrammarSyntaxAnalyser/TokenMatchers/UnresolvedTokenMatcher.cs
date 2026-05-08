using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class UnresolvedTokenMatcher(string temporaryName) : TokenMatcher(temporaryName)
    {
        TokenMatcher? matcher = null;

        public override string Name
        {
            get => matcher == null ? base.Name : matcher.Name;
            set
            {
                if (matcher == null) base.Name = value;
                else matcher.Name = value;
            }
        }

        public override Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program)
        {
            if (matcher == null) throw new InvalidOperationException($"Matcher \"{Name}\" is Unresolved");
            return matcher.TryMatch(tokens, program);
        }

        public void Resolve(TokenMatcher matcher) => this.matcher = matcher;
    }
}
