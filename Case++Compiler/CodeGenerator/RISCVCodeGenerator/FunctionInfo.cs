using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.CodeGenerator.RISCVCodeGenerator
{
    internal class FunctionInfo
    {
        internal List<StackFrame> StackFrames { get; } = [];

        internal void AddStackFrameFromScope(Scope scope)
        {
            lock (scope.Symbols)
            {
                StackFrame frame = new(scope.Start, scope.IsBase ? StackFrame.baseFrameExtraBytes : StackFrames[^1].SkipOffset,
                    from symbol in scope.Symbols
                    let variable = symbol as Variable
                    where variable != null
                    select variable);
                scope.SymbolAdded += (sender, symbol) =>
                {
                    if (symbol is Variable v) frame.AddVariable(v);
                };
                scope.Ended += (sender, end) => frame.End = end;
                StackFrames.Add(frame);
            }
        }
    }
}
