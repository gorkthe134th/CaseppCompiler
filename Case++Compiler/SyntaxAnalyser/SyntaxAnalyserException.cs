namespace CaseppCompiler.SyntaxAnalyser
{
    public class SyntaxAnalyserException : CompilerException
    {
        public SyntaxAnalyserException(Position position, string? message)
            : base(position, message) { }

        public SyntaxAnalyserException(Position position, string? message, Exception? innerException)
            : base(position, message, innerException) { }
    }
}
