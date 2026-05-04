using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.CodeGenerator.RISCVCodeGenerator
{
    internal record class FunctionInfo(TaskCompletionSource Ready, List<StackFrame> StackFrames)
    {
        private Dictionary<Scope, ScopeBasedEvents> events = [];

        private class ScopeBasedEvents
        {
            public delegate void StackFrameAddedHandler(StackFrame stackFrame);
            public event StackFrameAddedHandler? StackFrameAdded;

            public void InvokeStackFrameAdded(StackFrame stackFrame)
            {
                StackFrameAdded?.Invoke(stackFrame);
                StackFrameAdded = null;
            }
        }

        internal void AddStackFrameFromScope(Scope scope)
        {
            if (Ready.Task.IsCompleted)
                throw new ArgumentException($"Function \"{scope.EncompassingFunction}\" has been finalized, but got unprocessed scope for it: {scope}");

            StackFrame frame = new(scope.Start, scope.End ?? throw new InvalidOperationException("Scope unfinished."), scope.IsBase,
                from symbol in scope.Symbols
                let variable = symbol as Variable
                where variable != null
                select variable);

            StackFrames.Add(frame);
            GetOrAddEvents(scope).InvokeStackFrameAdded(frame);

            if (scope.IsBase)
            {
                events.Clear();
                Ready.SetResult();
                return;
            }

            if (scope.Parent != null) GetOrAddEvents(scope.Parent).StackFrameAdded += frame.HandleStackFrameForParentAdded;
        }

        private ScopeBasedEvents GetOrAddEvents(Scope scope)
        {
            if (!events.TryGetValue(scope, out var scopeEvents)) events.Add(scope, scopeEvents = new());
            return scopeEvents;
        }
    }
}
