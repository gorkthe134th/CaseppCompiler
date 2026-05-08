using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Reflection;

namespace CaseppCompiler.IntermediateProgramWriter
{
    public class ScopeWriterImplementation : IIntermediateProgramWriter
    {
        private record class WriteContext(StreamWriter Writer) : IDisposable
        {
            public void Dispose() => Writer.Dispose();
        }

        public Task Write(IntermediateProgram input, Stream ouput)
        {
            TaskCompletionSource taskCompletionSource = new();
            WriteContext context = new(new(ouput));
            input.Scopes.ItemAdded += (sender, scope) => SubscribeToScope(scope, context);
            input.Scopes.Completed += (sender) =>
            {
                context.Dispose();
                taskCompletionSource.SetResult();
            };
            return taskCompletionSource.Task;
        }

        private static void SubscribeToScope(Scope scope, WriteContext context) =>
            scope.Ended += (sender, end) => WriteScope(sender, end, context);

        private static void WriteScope(Scope scope, int end, WriteContext context) =>
            context.Writer.WriteLine($"{scope.EncompassingFunction.Name} {(scope.IsBase ? '#' : '|')} {scope.Start} - {end}: {string.Join(", ", scope.Symbols.Select(s => s.Name))}");
    }
}
