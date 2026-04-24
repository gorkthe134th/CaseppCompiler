using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal interface IFormalParameter
    {
        public bool IsMatch(IActualParameter actualParameter, [NotNullWhen(false)] out string? errorMessage);
    }

    internal partial struct TypeRestrictedFormalParameter<T> : IFormalParameter where T : IActualParameter
    {
        [GeneratedRegex(@"(\B[A-Z])")]
        private static partial Regex TypeNameFormatRegex { get; }

        public readonly bool IsMatch(IActualParameter actualParameter, [NotNullWhen(false)] out string? errorMessage)
        {
            if (actualParameter is T)
            {
                errorMessage = null;
                return true;
            }
            errorMessage = $"Expected {TypeNameFormatRegex.Replace(typeof(T).Name, " $1")}";
            return false;
        }
    }
}
