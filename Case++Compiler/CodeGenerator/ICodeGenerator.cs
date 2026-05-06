using CaseppCompiler.CodeGenerator.RISCVCodeGenerator;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

namespace CaseppCompiler.CodeGenerator
{
    public interface ICodeGenerator
    {
        public void Analyse(IntermediateProgram input, CodeStream? output = null);
    }

    public static class CodeGeneratorFactory
    {
        public static ICodeGenerator Create(string type = "") =>
            type switch
            {
                "riscv" => new RISCVCodeGeneratorImplementation(),
                _ => new RISCVCodeGeneratorImplementation(),
            };
    }
}
