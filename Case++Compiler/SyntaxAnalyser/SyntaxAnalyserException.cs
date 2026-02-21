namespace CaseppCompiler.SyntaxAnalyser
{
    public class SyntaxAnalyserException : Exception
    {
        public SyntaxAnalyserException() : base() { }

        public SyntaxAnalyserException(string? message) : base(message) { }

        public SyntaxAnalyserException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
