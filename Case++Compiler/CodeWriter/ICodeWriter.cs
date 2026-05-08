namespace CaseppCompiler.CodeWriter
{
    public interface ICodeWriter
    {
        public Task Write(Stream<string> input, Stream ouput);
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
