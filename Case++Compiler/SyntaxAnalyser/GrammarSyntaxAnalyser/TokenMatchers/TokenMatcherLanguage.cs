using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    public static class TokenMatcherLanguage
    {
        extension(string)
        {
            public static TokenMatcher operator ~(string name) => new ImpossibleTokenMatcher(name);

            public static TokenMatcher operator -(string name) => new AlwaysPassTokenMatcher(name);

            public static TokenMatcher operator %(string name, TokenMatcher matcher)
            {
                if (matcher.IsGenerated)
                {
                    matcher.Name = name;
                    matcher.IsGenerated = false;
                    return matcher;
                }
                return new RenameTokenMatcher(name, matcher);
            }

            public static TokenMatcher operator >>(string name, TokenMatcher matcher) =>
                new BlockTokenMatcher(name, matcher);

            public static TokenMatcher operator >(string name, TokenMatcher matcher) =>
                new ParenthesesTokenMatcher(name, matcher);

            public static TokenMatcher operator <(string name, TokenMatcher matcher) =>
                throw new InvalidOperationException();

            public static TokenMatcher operator >=(string name, TokenMatcher matcher) =>
                new SquareBracketTokenMatcher(name, matcher);

            public static TokenMatcher operator <=(string name, TokenMatcher matcher) =>
                throw new InvalidOperationException();

            public static TokenMatcher operator ^(string name, TokenMatcher matcher) =>
                new OptionalTokenMatcher(name, matcher);

            public static TokenMatcher operator *(string name, TokenMatcher matcher) =>
                new KleeneStarTokenMatcher(name, matcher);

            public static TokenMatcher operator |(string name, TokenMatcher[] matcherSequence) =>
                new AlternativeTokenMatcher(name, matcherSequence);
        }

        extension(TokenMatcher)
        {
            public static TokenMatcher operator |(TokenMatcher matcher, Func<IntermediateProgram, Task> finalAction)
            {
                matcher.FinalAction = finalAction;
                return matcher;
            }
        }
    }
}
