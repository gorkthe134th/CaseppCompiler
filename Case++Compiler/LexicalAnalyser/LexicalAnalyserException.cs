namespace CaseppCompiler.LexicalAnalyser
{
    public class LexicalAnalyserException : CompilerException
    {
        public LexicalAnalyserException(Position position, string? message)
            : base(position, message) { }

        public LexicalAnalyserException(Position position, string? message, Exception? innerException)
            : base(position, message, innerException) { }
    }
}
