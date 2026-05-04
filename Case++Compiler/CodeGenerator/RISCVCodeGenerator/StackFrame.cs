using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.CodeGenerator.RISCVCodeGenerator
{
    internal class StackFrame
    {
        private const int baseFrameExtraBytes = 8;

        internal int Length { get; }

        internal int Start { get; }

        internal int End { get; }

        private int firstVariableOffset;
        private readonly Dictionary<Variable, int> variableOffsets;

        internal delegate void MovedHandler(StackFrame sender);
        internal event MovedHandler? Moved;

        internal StackFrame(int start, int end, bool isBase, params IEnumerable<Variable> variables)
        {
            firstVariableOffset = isBase ? baseFrameExtraBytes : 0;

            int length = 0;
            variableOffsets = variables.Select((v, i) =>
            {
                var kvp = new KeyValuePair<Variable, int>(v, length);
                length += 4;
                return kvp;
            }).ToDictionary();

            Length = firstVariableOffset + length;
            Start = start;
            End = end;
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

        internal void HandleStackFrameForParentAdded(StackFrame stackFrame)
        {
            UpdateOffset(stackFrame);
            stackFrame.Moved += UpdateOffset;
        }

        private void UpdateOffset(StackFrame parent)
        {
            firstVariableOffset = parent.firstVariableOffset + parent.Length;
            Moved?.Invoke(this);
        }
    }
}
