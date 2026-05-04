using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.IntermediateProgramWriter
{
    public class ScopeWriterImplementation : IIntermediateProgramWriter
    {
        public void Write(IntermediateProgram input, Stream ouput, IntermediateProgram? forward = null)
        {
            using StreamWriter writer = new(ouput);
            foreach (var scope in input.Scopes.GetConsumingEnumerable())
            {
                writer.WriteLine($"{scope.EncompassingFunction.Name} {(scope.IsBase ? '#' : '|')} {scope.Start} - {scope.End}: {string.Join(", ", scope.Symbols.Select(s => s.Name))}");
                forward?.Scopes.Add(scope);
            }
            forward?.Scopes.CompleteAdding();
        }
    }
}
