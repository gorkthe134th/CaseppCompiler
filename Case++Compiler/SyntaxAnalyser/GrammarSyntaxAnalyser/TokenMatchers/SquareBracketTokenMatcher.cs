using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class SquareBracketTokenMatcher(string name, TokenMatcher contentMatcher) : TokenMatcher(name)
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
        {
            if (tokens.Current is not SquareBracketToken startToken ||
                startToken.Type != RegionMarkType.Start) return false;
            MoveNext(tokens);

            if (contentMatcher.TryMatch(tokens, program) == false) throw new SyntaxAnalyserException($"Expected {Name}: {tokens.Current}");

            if (tokens.Current is not SquareBracketToken endToken ||
                endToken.Type != RegionMarkType.End) throw new SyntaxAnalyserException($"Expected Close Square Bracket Token: {tokens.Current}");
            MoveNext(tokens);

            return true;
        }
    }
}
