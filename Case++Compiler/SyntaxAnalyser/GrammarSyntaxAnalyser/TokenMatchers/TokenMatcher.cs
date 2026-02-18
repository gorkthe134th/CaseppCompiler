using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Collections;
using System.Runtime.CompilerServices;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    [CollectionBuilder(typeof(TokenMatcher), "Create")]
    internal abstract class TokenMatcher(string name)
    {
        public virtual string Name { get; set; } = name;

        public abstract bool CanMatchEmpty { get; }

        public abstract bool CanMatch(Token firstToken);

        public abstract void Match(IEnumerator<Token> tokens);

        public static implicit operator TokenMatcher(Type type) =>
            (TokenMatcher?)typeof(TypeTokenMatcher<>).MakeGenericType(type).GetConstructor([typeof(string)])?.Invoke([GenerateName()]) ??
            throw new InvalidOperationException("No suitable TypeTokenMatcher Constructor exists");

        public static TokenMatcher Create(ReadOnlySpan<TokenMatcher> matchers) =>
            new SequenceTokenMatcher(GenerateName(), matchers.ToArray());

        private static int id = 0;
        private static string GenerateName() => $"Matcher{++id}";

        // This method is required for initialisation using a Collection Expression
        public IEnumerator<TokenMatcher> GetEnumerator() => throw new NotSupportedException();
    }
}
