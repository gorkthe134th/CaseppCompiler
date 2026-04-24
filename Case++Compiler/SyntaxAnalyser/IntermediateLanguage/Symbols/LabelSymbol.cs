using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols
{
    internal record class LabelSymbol : Symbol
    {
        // TODO: Replace with a union when C# 15 is officially released
        private int? position;
        private IList<JumpInstruction>? jumpsToLabel;

        private LabelSymbol(string name) : base(name) { }

        public LabelSymbol(string name, IList<JumpInstruction> jumpsToLabel) : this(name)
        {
            this.position = null;
            this.jumpsToLabel = jumpsToLabel;
        }

        public LabelSymbol(string name, int position) : this(name)
        {
            this.position = position;
            this.jumpsToLabel = null;
        }

        public void AddJump(JumpInstruction jump)
        {
            if (position is not int labelPosition) jumpsToLabel?.Add(jump);
            else jump.Target = labelPosition;
        }

        public bool TrySet(int position)
        {
            if (this.position != null) return false;
            this.jumpsToLabel?.Targets = position;
            this.position = position;
            return true;
        }
    }
}
