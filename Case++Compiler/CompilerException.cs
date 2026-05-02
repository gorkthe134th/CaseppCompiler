namespace CaseppCompiler
{
    public class CompilerException : Exception
    {
        public CompilerException(Position position, string? message)
            : base($"Line {position.Line}, Column {position.Column}: {message}") { }

        public CompilerException(Position position, string? message, Exception? innerException)
            : base($"Line {position.Line}, Column {position.Column}: {message}", innerException) { }
    }
}
