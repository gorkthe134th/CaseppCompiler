using System.Threading.Channels;

namespace CaseppCompiler
{
    public class Stream<T>(int? capacity = null, CancellationToken? cancellationToken = null)
    {
        private readonly Channel<T> items = capacity == null ? Channel.CreateUnbounded<T>() : Channel.CreateBounded<T>((int)capacity);
        private readonly TaskCompletionSource finish = new();
        private bool isCompleted = false;

        public delegate void ItemAddingHandler(Stream<T> sender, T item);
        public event ItemAddingHandler? ItemAdding;

        public delegate void ItemAddedHandler(Stream<T> sender, T item);
        public event ItemAddedHandler? ItemAdded;

        public delegate void ItemTakenHandler(Stream<T> sender, T item);
        public event ItemTakenHandler? ItemTaken;

        public int Count => items.Reader.Count;

        public bool IsCompleted => isCompleted;

        public Task Finish => finish.Task;

        public async Task AddAsync(T item)
        {
            ItemAdding?.Invoke(this, item);
            await items.Writer.WriteAsync(item, cancellationToken ?? default);
            ItemAdded?.Invoke(this, item);
        }

        public async Task<T> TakeAsync(Func<T, Task>? waitReady = null)
        {
            T item = await items.Reader.ReadAsync(cancellationToken ?? default);
            if (waitReady != null) await waitReady(item);
            ItemTaken?.Invoke(this, item);
            if (Count == 0 && isCompleted) finish.TrySetResult();
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
            if (cancellationToken?.IsCancellationRequested == true) finish.TrySetCanceled();
            else finish.TrySetResult();
        }

        public void Complete()
        {
            items.Writer.Complete();
            isCompleted = true;
            if (Count == 0) finish.TrySetResult();
        }
    }
}
