using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.Writers.IntermediateProgramWriter
{
    public class ScopeWriterImplementation : IIntermediateProgramWriter
    {
        public Task Write(IntermediateProgram input, Stream ouput, CancellationToken? cancellationToken = null)
        {
            if (input.Scopes.Count > 0) throw new ArgumentException("Input already contains Scopes.");
            StreamWriter writer = new(ouput);
            input.Scopes.ItemAdded += SubscribeToScope;
            return input.Scopes.Finish.ContinueWith(_ => writer.Dispose());

            void SubscribeToScope(Stream<Scope> sender, Scope scope)
            {
                scope.Ended += WriteScope;
            }

            void WriteScope(Scope scope, int end)
            {
                writer.WriteLine($"{scope.EncompassingFunction.Name} {(scope.IsBase ? '#' : '|')} {scope.Start} - {end}: {string.Join(", ", scope.Symbols.Select(s => s.Name))}");
            }
        }
    }
}
