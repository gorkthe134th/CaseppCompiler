using CaseppCompiler.CodeGenerator;

namespace CaseppCompiler.CodeWriter
{
    public interface ICodeWriter
    {
        public void Write(CodeStream input, Stream ouput, CodeStream? forward = null);
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
