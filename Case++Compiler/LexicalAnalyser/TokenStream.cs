using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Collections.Concurrent;

namespace CaseppCompiler.LexicalAnalyser
{
    public class TokenStream(int capacity, CancellationToken? cancellationToken) : IDisposable
    {
        private readonly BlockingCollection<Token> tokens = new(new ConcurrentQueue<Token>(), boundedCapacity: capacity);

        public void Add(Token token)
        {
            if (cancellationToken == null) tokens.Add(token);
            else tokens.Add(token, (CancellationToken)cancellationToken);
        }

        public Token Take() =>
            cancellationToken != null
            ? tokens.Take((CancellationToken)cancellationToken)
            : tokens.Take();

        public IEnumerable<Token> GetConsumingEnumerable() =>
            cancellationToken != null
            ? tokens.GetConsumingEnumerable((CancellationToken)cancellationToken)
            : tokens.GetConsumingEnumerable();

        public void CompleteAdding() => tokens.CompleteAdding();

        public void Dispose() => tokens.Dispose();
    }
}
