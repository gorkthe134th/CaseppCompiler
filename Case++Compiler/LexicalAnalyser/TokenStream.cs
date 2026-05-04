using CaseppCompiler.LexicalAnalyser.Tokens;

using System.Collections.Concurrent;

namespace CaseppCompiler.LexicalAnalyser
{
    public class TokenStream : IDisposable
    {
        private readonly BlockingCollection<Token> tokens;
        private readonly CancellationToken? cancellationToken;

        public TokenStream(int? capacity = null, CancellationToken? cancellationToken = null)
        {
            this.cancellationToken = cancellationToken;
            ConcurrentQueue<Token> queque = new();
            tokens = capacity == null ? new(queque) : new(queque, boundedCapacity: (int)capacity);
        }

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
