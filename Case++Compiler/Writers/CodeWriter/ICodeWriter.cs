namespace CaseppCompiler.Writers.CodeWriter
{
    public interface ICodeWriter
    {
        public Task Write(Stream<string> input, Stream ouput, CancellationToken? cancellationToken = null);
    }

    public static class CodeWriterFactory
    {
        public static ICodeWriter Create(string type = "") =>
            type switch
            {
                "consume" => new ConsumingCodeWriterImplementation(),
                _ => new ConsumingCodeWriterImplementation(),
            };
    }
}
