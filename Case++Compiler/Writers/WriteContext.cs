namespace CaseppCompiler.Writers
{
    internal class WriteContext
    {
        private readonly StreamWriter writer;
        private readonly TaskCompletionSource writeComplete = new();
        private int userCount = 0;
        private bool allowDispose = false;

        public WriteContext(StreamWriter writer, CancellationToken? cancellationToken = null)
        {
            this.writer = writer;
            cancellationToken?.Register(() =>
            {
                lock (this)
                {
                    this.writeComplete.SetCanceled();
                    this.writer.Dispose();
                }
            });
        }

        public Task WriteComplete => writeComplete.Task;

        public void UseWriter(Action<StreamWriter> action)
        {
            lock (this)
            {
                action.Invoke(writer);
            }
        }

        public void AddUser()
        {
            lock (this)
            {
                if (writeComplete.Task.IsCanceled) return;
                userCount++;
            }
        }

        public void RemoveUser()
        {
            lock (this)
            {
                if (writeComplete.Task.IsCanceled) return;
                userCount--;
                if (userCount < 0) throw new InvalidOperationException("There are no users to be removed.");
                if (userCount == 0 && allowDispose)
                {
                    writeComplete.SetResult();
                    writer.Dispose();
                }
            }
        }

        public void AllowDispose()
        {
            lock (this)
            {
                if (writeComplete.Task.IsCanceled) return;
                allowDispose = true;
                if (userCount == 0)
                {
                    writeComplete.SetResult();
                    writer.Dispose();
                }
            }
        }
    }
}
