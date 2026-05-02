using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal abstract record FormalParameter(Variable AssociatedVariable)
    {
        public abstract void Match(ActualParameter actualParameter);

        public class MismatchException : Exception
        {
            public MismatchException() { }

            public MismatchException(string? message) : base(message) { }

            public MismatchException(string? message, Exception? innerException) : base(message, innerException) { }
        }
    }

    internal partial record TypeRestrictedFormalParameter<T>(Variable AssociatedVariable) : FormalParameter(AssociatedVariable) where T : ActualParameter
    {
        [GeneratedRegex(@"(\B[A-Z])")]
        private static partial Regex TypeNameFormatRegex { get; }

        private static string FormattedTypeName => TypeNameFormatRegex.Replace(typeof(T).Name, " $1");

        public override void Match(ActualParameter actualParameter)
        {
            if (actualParameter is not T)
                throw new MismatchException($"Expected {FormattedTypeName}");
        }

        public override string ToString() => $"{FormattedTypeName} {AssociatedVariable.Name}";
    }
}
