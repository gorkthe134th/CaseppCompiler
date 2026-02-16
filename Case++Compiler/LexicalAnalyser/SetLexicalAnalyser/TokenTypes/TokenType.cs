using CaseppCompiler.LexicalAnalyser.Tokens;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal abstract class TokenType
    {
        /// <summary>
        /// A sequence of Predicates that all need to pass for the Token to be prepresentative of a sequence of characters.
        /// Each Predicate must be evaluated before moving to the next Predicate.
        /// A return value of <c>true</c> indicates that the character is required.
        /// A return value of <c>null</c> indicates that the character is acceptable and no more characters are required.
        /// A return value of <c>false</c> indicates that the character is not acceptable.
        /// Predicates produced by implementations of this Property must not return <c>true</c> after a previous Predicate in the same sequence has returned <c>null</c>.
        /// Predicates produced by implementations of this Property are allowed to have different return values based on parameters passed to previous Predicates in the same sequence.
        /// </summary>
        public abstract IEnumerable<Func<char, bool?>> CharacterPredicates { get; }

        public abstract int Limit { get; }

        public abstract Token? GenerateToken(string text, int line, int column);
    }
}
