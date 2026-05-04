using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.IntermediateProgramWriter
{
    public class FunctionWriterImplementation : IIntermediateProgramWriter
    {
        public void Write(IntermediateProgram input, Stream ouput, IntermediateProgram? forward = null)
        {
            using StreamWriter writer = new(ouput);
            int line = 0;
            foreach (var function in input.Functions.GetConsumingEnumerable())
            {
                foreach (var quad in function.ToQuads(line))
                    writer.WriteLine($"{line++}: {quad.Item1 ?? "_"}, {quad.Item2 ?? "_"}, {quad.Item3 ?? "_"}, {quad.Item4 ?? "_"}");
                forward?.Functions.Add(function);
            }
            forward?.Functions.CompleteAdding();
        }
    }
}
