using CaseppCompiler.CodeGenerator;

namespace CaseppCompiler.CodeWriter
{
    public class SimpleCodeWriterImplementation : ICodeWriter
    {
        public void Write(CodeStream input, Stream ouput, CodeStream? forward = null)
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
