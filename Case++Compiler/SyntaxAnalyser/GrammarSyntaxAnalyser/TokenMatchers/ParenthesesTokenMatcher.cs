using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    internal class ParenthesesTokenMatcher(string name, TokenMatcher contentMatcher) : TokenMatcher(name)
    {
        public override bool? BaseTryMatch(IEnumerator<Token> tokens, IntermediateProgram? program)
        {
            if (tokens.Current is not ParenthesisToken startToken ||
                startToken.Type != RegionMarkType.Start) return false;
            MoveNext(tokens);

            if (contentMatcher.TryMatch(tokens, program) == false)
                throw new SyntaxAnalyserException(tokens.Current.Position, $"Expected {Name}, but got {tokens.Current}.");

            if (tokens.Current is not ParenthesisToken endToken || endToken.Type != RegionMarkType.End)
                throw new SyntaxAnalyserException(tokens.Current.Position, $"Expected Close Parenthesis Token, but got {tokens.Current}.");
            MoveNext(tokens);

            return true;
        }
    }
}
