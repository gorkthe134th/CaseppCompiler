using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace CaseppCompiler
{
    public class Stream<T>
    {
        private readonly Channel<T> items;
        private readonly CancellationToken? cancellationToken;

        public delegate void ItemAddedHandler(Stream<T> sender, T item);
        public event ItemAddedHandler? ItemAdded;

        public delegate void ItemTakenHandler(Stream<T> sender, T item);
        public event ItemTakenHandler? ItemTaken;

        public delegate void CompletedHandler(Stream<T> sender);
        public event CompletedHandler? Completed;

        public Stream(int? capacity = null, CancellationToken? cancellationToken = null)
        {
            this.cancellationToken = cancellationToken;
            items = capacity == null ? Channel.CreateUnbounded<T>() : Channel.CreateBounded<T>((int)capacity);
        }

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

        internal bool TryTake([NotNullWhen(true)] out T? item, Func<T, bool>? ready = null)
        {
            if (!items.Reader.TryRead(out item)) return false;
            if (ready != null && !ready(item)) return false;
            ItemTaken?.Invoke(this, item);
            return true;
        }
        // The warning here is caused by the method used having a "MaybeNullWhen(false)" Attribute instead of "NotNullWhen(true)".
        // Technically, the two Attributes do not imply the same thing, but the intention is the same.

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
