using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.Writers.IntermediateProgramWriter
{
    public class ScopeWriterImplementation : IIntermediateProgramWriter
    {
        public Task Write(IntermediateProgram input, Stream ouput, CancellationToken? cancellationToken = null)
        {
            WriteContext context = new(new(ouput), cancellationToken);
            input.Scopes.ItemAdded += (sender, scope) => SubscribeToScope(scope, context);
            input.Scopes.Completed += (sender) => context.AllowDispose();
            return context.WriteComplete;
        }

        private static void SubscribeToScope(Scope scope, WriteContext context)
        {
            context.AddUser();
            scope.Ended += (sender, end) => WriteScope(sender, end, context);
        }

        private static void WriteScope(Scope scope, int end, WriteContext context)
        {
            context.UseWriter(writer =>
                writer.WriteLine($"{scope.EncompassingFunction.Name} {(scope.IsBase ? '#' : '|')} {scope.Start} - {end}: {string.Join(", ", scope.Symbols.Select(s => s.Name))}"));
            context.RemoveUser();
        }
    }
}
