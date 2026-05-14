using CaseppCompiler.CodeGenerator.RISCVCodeGenerator;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.CodeGenerator
{
    public interface ICodeGenerator
    {
        public Task Analyse(IntermediateProgram input, Stream<string>? output = null, CancellationToken? cancellationToken = null);
    }

    public static class CodeGeneratorFactory
    {
        public static ICodeGenerator Create(string type = "") =>
            type switch
            {
                "riscv" => new RISCVCodeGeneratorImplementation(),
                _ => Create("riscv"),
            };
    }
}
