namespace CaseppCompiler.CodeGenerator
{
    public class CodeGeneratorException : CompilerException
    {
        public CodeGeneratorException(Position position, string? message)
            : base(position, message) { }

        public CodeGeneratorException(Position position, string? message, Exception? innerException)
            : base(position, message, innerException) { }
    }
}
