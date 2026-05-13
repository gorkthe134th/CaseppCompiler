namespace CaseppCompiler
{
    internal class OperationMonitor
    {
        private readonly TaskCompletionSource taskCompletionSource = new();
        private int currentCount = 0;
        private bool allowCompletion = false;

        public int CurrentCount { get => currentCount; set => currentCount = value; }

        public event Action? Completed;

        public OperationMonitor(CancellationToken? cancellationToken = null)
        {
            cancellationToken?.Register(() =>
            {
                lock (this)
                {
                    if (taskCompletionSource.TrySetCanceled()) Completed?.Invoke();
                }
            });
        }

        public Task WaitAsync() => taskCompletionSource.Task;

        public void PerformOperation(Action operation)
        {
            lock (this)
            {
                operation.Invoke();
            }
        }

        public void Add(Action? firstOperation = null, int count = 1)
        {
            lock (this)
            {
                if (taskCompletionSource.Task.IsCanceled) return;
                currentCount += count;
                firstOperation?.Invoke();
            }
        }

        public void Remove(Action? finalOperation = null, int count = 1)
        {
            lock (this)
            {
                if (taskCompletionSource.Task.IsCanceled) return;
                finalOperation?.Invoke();
                currentCount -= count;
                if (currentCount < 0) throw new InvalidOperationException("Operation resulted in negative count.");
                if (currentCount == 0 && allowCompletion) Complete();
            }
        }

        public void AllowCompletion()
        {
            lock (this)
            {
                if (taskCompletionSource.Task.IsCanceled) return;
                allowCompletion = true;
                if (currentCount == 0) Complete();
            }
        }

        private void Complete()
        {
            if(taskCompletionSource.TrySetResult()) Completed?.Invoke();
        }
    }
}
