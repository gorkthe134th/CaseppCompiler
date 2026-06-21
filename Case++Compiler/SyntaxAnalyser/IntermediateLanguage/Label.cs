using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    internal class Label : Symbol
    {
        // TODO: Replace with a union when C# 15 is officially released
        private int? position;
        private IList<JumpInstruction>? jumpsToLabel;

        private Label(string name) : base(name) { }

        public Label(string name, IList<JumpInstruction> jumpsToLabel) : this(name)
        {
            this.position = null;
            this.jumpsToLabel = jumpsToLabel;
        }

        public Label(string name, int position) : this(name)
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

        internal override void ForgetFunction()
        {
            // TODO: Consider adding Label Bubbling between Scopes to allow jumping to later labels.
            if (position == null) throw new InvalidOperationException("This Label has not been set.");
            base.ForgetFunction();
        }
    }
}
