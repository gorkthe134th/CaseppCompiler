using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace CaseppCompiler
{
    public class Stream<T>(int? capacity = null, CancellationToken? cancellationToken = null)
    {
        private readonly Channel<T> items = capacity == null ? Channel.CreateUnbounded<T>() : Channel.CreateBounded<T>((int)capacity);
        private readonly CancellationToken? cancellationToken = cancellationToken;

        public delegate void ItemAddedHandler(Stream<T> sender, T item);
        public event ItemAddedHandler? ItemAdded;

        public delegate void ItemTakenHandler(Stream<T> sender, T item);
        public event ItemTakenHandler? ItemTaken;

        public delegate void CompletedHandler(Stream<T> sender);
        public event CompletedHandler? Completed;

        public int Count => items.Reader.Count;

        public async Task AddAsync(T item)
        {
            await items.Writer.WriteAsync(item, cancellationToken ?? default);
            ItemAdded?.Invoke(this, item);
        }

        public async Task<T> TakeAsync(Func<T, Task>? waitReady = null)
        {
            T item = await items.Reader.ReadAsync(cancellationToken ?? default);
            if (waitReady != null) await waitReady(item);
            ItemTaken?.Invoke(this, item);
            return item;
        }

        public async IAsyncEnumerable<T> GetAsyncEnumerable(Func<T, Task>? waitReady = null)
        {
            await foreach (T item in items.Reader.ReadAllAsync(cancellationToken ?? default))
            {
                if (waitReady != null) await waitReady(item);
                ItemTaken?.Invoke(this, item);
                yield return item;
            }
        }

        public void Complete()
        {
            items.Writer.Complete();
            Completed?.Invoke(this);
        }
    }
}
