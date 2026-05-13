using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.Writers.IntermediateProgramWriter
{
    public class ScopeWriterImplementation : IIntermediateProgramWriter
    {
        public Task Write(IntermediateProgram input, Stream ouput, CancellationToken? cancellationToken = null)
        {
            if (input.Scopes.Count > 0) throw new ArgumentException("Input already contains Scopes.");
            OperationMonitor operationMonitor = new(cancellationToken);
            StreamWriter writer = new(ouput);
            input.Scopes.ItemAdded += SubscribeToScope;
            input.Scopes.Completed += (sender) => operationMonitor.AllowCompletion();
            operationMonitor.Completed += writer.Dispose;
            return operationMonitor.WaitAsync();

            void SubscribeToScope(Stream<Scope> sender, Scope scope)
            {
                operationMonitor.Add();
                scope.Ended += WriteScope;
            }

            void WriteScope(Scope scope, int end)
            {
                operationMonitor.Remove(() =>
                    writer.WriteLine($"{scope.EncompassingFunction.Name} {(scope.IsBase ? '#' : '|')} {scope.Start} - {end}: {string.Join(", ", scope.Symbols.Select(s => s.Name))}"));
            }
        }
    }
}
