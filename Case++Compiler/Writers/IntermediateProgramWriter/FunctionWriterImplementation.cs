using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.Writers.IntermediateProgramWriter
{
    public class FunctionWriterImplementation : IIntermediateProgramWriter
    {
        public Task Write(IntermediateProgram input, Stream ouput, CancellationToken? cancellationToken = null)
        {
            if (input.Functions.Count > 0) throw new ArgumentException("Input already contains Functions.");
            StreamWriter writer = new(ouput);
            return input.ToQuadsEvents((line, quad) =>
                writer.WriteLine($"{line}: {quad.Item1 ?? "_"}, {quad.Item2 ?? "_"}, {quad.Item3 ?? "_"}, {quad.Item4 ?? "_"}"),
                cancellationToken).ContinueWith(_ => writer.Dispose());
        }
    }
}
