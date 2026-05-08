using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SquareBracketTokenMatcher(string name, TokenMatcher contentMatcher) : TokenMatcher(name)
    {
        public override async Task<bool?> BaseTryMatch(IAsyncEnumerator<Token> tokens, IntermediateProgram? program)
        {
            if (tokens.Current is not SquareBracketToken startToken ||
                startToken.Type != RegionMarkType.Start) return false;
            await MoveNext(tokens);

            if (await contentMatcher.TryMatch(tokens, program) == false)
                throw new SyntaxAnalyserException(tokens.Current.Position, $"Expected {Name}, but got {tokens.Current}.");

            if (tokens.Current is not SquareBracketToken endToken || endToken.Type != RegionMarkType.End)
                throw new SyntaxAnalyserException(tokens.Current.Position, $"Expected Close Square Bracket Token, but got {tokens.Current}.");
            await MoveNext(tokens);

            return true;
        }
    }
}
