using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.Writers.IntermediateProgramWriter
{
    public interface IIntermediateProgramWriter
    {
        public Task Write(IntermediateProgram input, Stream ouput, CancellationToken? cancellationToken = null);
    }

    public static class IntermediateProgramWriterFactory
    {
        public static IIntermediateProgramWriter Create(string type = "") =>
            type switch
            {
                "int" => new FunctionWriterImplementation(),
                "sym" => new ScopeWriterImplementation(),
                _ => new FunctionWriterImplementation(),
            };
    }
}
