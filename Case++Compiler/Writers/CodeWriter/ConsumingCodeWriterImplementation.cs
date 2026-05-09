namespace CaseppCompiler.Writers.CodeWriter
{
    public class ConsumingCodeWriterImplementation : ICodeWriter
    {
        public async Task Write(Stream<string> input, Stream ouput, CancellationToken? cancellationToken = null)
        {
            using StreamWriter writer = new(ouput);
            await foreach (var line in input.GetAsyncEnumerable())
                writer.WriteLine(line);
        }
    }
}
