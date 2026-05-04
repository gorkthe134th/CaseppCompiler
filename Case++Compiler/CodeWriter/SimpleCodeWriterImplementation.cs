using System.Collections.Concurrent;

namespace CaseppCompiler.CodeWriter
{
    public class SimpleCodeWriterImplementation : ICodeWriter
    {
        public void Write(BlockingCollection<string> input, Stream ouput, BlockingCollection<string>? forward = null)
        {
            using StreamWriter writer = new(ouput);
            foreach (var line in input.GetConsumingEnumerable())
            {
                writer.WriteLine(line);
                forward?.Add(line);
            }
            forward?.CompleteAdding();
        }
    }
}
