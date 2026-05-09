namespace CaseppCompiler.Writers
{
    internal record class WriteContext(StreamWriter Writer)
    {
        private readonly TaskCompletionSource writeComplete = new();
        private int userCount = 0;
        private bool allowDispose = false;

        public Task WriteComplete => writeComplete.Task;

        public void AddUser()
        {
            lock (this)
            {
                userCount++;
            }
        }

        public void RemoveUser()
        {
            lock (this)
            {
                userCount--;
                if (userCount < 0) throw new InvalidOperationException("There are no users to be removed.");
                if (userCount == 0 && allowDispose)
                {
                    writeComplete.SetResult();
                    Writer.Dispose();
                }
            }
        }

        public void AllowDispose()
        {
            lock (this)
            {
                allowDispose = true;
                if (userCount == 0)
                {
                    writeComplete.SetResult();
                    Writer.Dispose();
                }
            }
        }
    }
}
