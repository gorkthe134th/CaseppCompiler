using CaseppCompiler.CodeGenerator.RISCVCodeGenerator;
using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;

using System.Collections.Concurrent;

namespace CaseppCompiler.CodeGenerator
{
    public interface ICodeGenerator
    {
        public void Analyse(IntermediateProgram input, BlockingCollection<string>? output = null);
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
