namespace CaseppCompiler.LexicalAnalyser
{
    public class LexicalAnalyserException : Exception
    {
        public LexicalAnalyserException() : base() { }

        public LexicalAnalyserException(string? message) : base(message) { }

        public LexicalAnalyserException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
