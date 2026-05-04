using System.Collections.Concurrent;

namespace CaseppCompiler.CodeWriter
{
    public interface ICodeWriter
    {
        public void Write(BlockingCollection<string> input, Stream ouput, BlockingCollection<string>? forward = null);
    }

    public static class CodeWriterFactory
    {
        public static ICodeWriter Create(string type = "") =>
            type switch
            {
                "simple" => new SimpleCodeWriterImplementation(),
                _ => new SimpleCodeWriterImplementation(),
            };
    }
}
