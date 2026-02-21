using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Collections;
using System.Runtime.CompilerServices;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers
{
    [CollectionBuilder(typeof(TokenMatcher), "Create")]
    public abstract class TokenMatcher(string name)
    {
        public virtual string Name { get; set; } = name;

        /// <summary>
        /// Tries to match the specified sequence.
        /// </summary>
        /// <param name="tokens">
        /// The sequence to match.
        /// </param>
        /// <returns>
        /// <c>true</c> if the match was successful,
        /// <c>false</c> if the match was not successful but it's possible to try a different <see cref="TokenMatcher"/>,
        /// <c>null</c> if the match was not successful but it's possible to skip this <see cref="TokenMatcher"/>.
        /// Throws <see cref="SyntaxAnalyserException"/> if the match was not successful and it's impossible to continue.
        /// </returns>
        public abstract bool? TryMatch(IEnumerator<Token> tokens);

        public static void MoveNext(IEnumerator<Token> tokens)
        {
            if (!tokens.MoveNext()) throw new SyntaxAnalyserException($"Expected EOF Token");
        }

        public static implicit operator TokenMatcher(Type type) =>
            (TokenMatcher?)typeof(TypeTokenMatcher<>).MakeGenericType(type).GetConstructor([typeof(string)])?.Invoke([GenerateName()]) ??
            throw new InvalidOperationException("No suitable TypeTokenMatcher Constructor exists");

        public static implicit operator TokenMatcher(OperatorToken.OperationType operation) =>
            new OperatorTokenMatcher(GenerateName(), operation);

        public static TokenMatcher Create(ReadOnlySpan<TokenMatcher> matchers) =>
            new SequenceTokenMatcher(GenerateName(), matchers.ToArray());

        private static int id = 0;
        private static string GenerateName() => $"Matcher{++id}";

        // This method is required for initialisation using a Collection Expression
        public IEnumerator<TokenMatcher> GetEnumerator() => throw new NotSupportedException();
    }
}
