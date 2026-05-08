using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.CodeGenerator.RISCVCodeGenerator
{
    internal class StackFrame
    {
        internal const int baseFrameExtraBytes = 8;

        internal int Length { get; private set; }

        internal int Start { get; }

        internal int? End { get; set; }

        private readonly int firstVariableOffset;
        private readonly Dictionary<Variable, int> variableOffsets;

        internal int SkipOffset => firstVariableOffset + Length; 

        internal StackFrame(int start, int offset, params IEnumerable<Variable> variables)
        {
            firstVariableOffset = offset;

            int length = 0;
            variableOffsets = variables.Select((v, i) =>
            {
                var kvp = new KeyValuePair<Variable, int>(v, length);
                length += 4;
                return kvp;
            }).ToDictionary();

            Length = length;
            Start = start;
            End = null;
        }

        internal void AddVariable(Variable variable)
        {
            try
            {
                variableOffsets.Add(variable, Length);
                Length += 4;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Variable \"{variable.Name}\" alraedy exists in this Stack Frame.", e);
            }
        }

        internal int GetOffset(Variable variable) => variableOffsets[variable] + firstVariableOffset;

        internal bool TryGetOffset(Variable variable, [NotNullWhen(true)] out int? offset)
        {
            if (!variableOffsets.TryGetValue(variable, out int variableOffset))
            {
                offset = null;
                return false;
            }
            offset = variableOffset + firstVariableOffset;
            return true;
        }
    }
}
