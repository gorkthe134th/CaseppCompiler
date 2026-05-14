using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace CaseppCompiler
{
    [CollectionBuilder(typeof(TaskList), "Create")]
    internal class TaskList : IDisposable, ICollection<Task>
    {
        private readonly List<Task> tasks;
        private TaskCompletionSource tasksChanged;
        private readonly SemaphoreSlim taskSemaphore;
        private readonly SemaphoreSlim whenAllSemaphore;
        private TaskCompletionSource whenAll;

        public int Count => tasks.Count;

        public bool IsReadOnly => false;

        public TaskList(params IEnumerable<Task> initialTasks)
        {
            tasksChanged = new();
            tasks = [tasksChanged.Task, .. initialTasks];
            taskSemaphore = new(1, 1);
            whenAllSemaphore = new(1, 1);
            whenAll = new();
            if (tasks.Count <= 1) whenAll.SetResult();
        }

        public static TaskList Create(ReadOnlySpan<Task> tasks) => new(tasks.ToArray());

        public bool Contains(Task item)
        {
            taskSemaphore.Wait();
            // Waiting synchronously is not a problem since taskSemaphore is entered for short periods of time
            // and does not depend on methods outside of this class. A thread entering it is guaranteed to exit soon.
            try
            {
                return tasks.Contains(item);
            }
            finally
            {
                taskSemaphore.Release();
            }
        }

        public void Add(Task task)
        {
            taskSemaphore.Wait();
            try
            {
                tasks.Add(task);
            }
            finally
            {
                if (whenAll.Task.IsCompleted) whenAll = new();
                else tasksChanged.TrySetResult();
                taskSemaphore.Release();
            }
        }

        public bool Remove(Task item)
        {
            taskSemaphore.Wait();
            try
            {
                return tasks.Remove(item);
            }
            finally
            {
                if (whenAll.Task.IsCompleted) whenAll = new();
                else tasksChanged.TrySetResult();
                taskSemaphore.Release();
            }
        }

        public void Clear()
        {
            taskSemaphore.Wait();
            try
            {
                tasks.RemoveAll(t => t != tasksChanged.Task);
            }
            finally
            {
                if (whenAll.Task.IsCompleted) whenAll = new();
                else tasksChanged.TrySetResult();
                taskSemaphore.Release();
            }
        }

        public async Task WhenAll()
        {
            if (whenAll.Task.IsCompleted) return;

            if (!await whenAllSemaphore.WaitAsync(0))
            {
                // Only one thread needs to enter the loop
                await whenAll.Task;
                return;
            }

            taskSemaphore.Wait();

            while (tasks.Count > 1)
            {
                var anyTaskComplete = Task.WhenAny(tasks).ConfigureAwait(false);

                taskSemaphore.Release();

                Task finishedTask = await anyTaskComplete;
                if (finishedTask.IsFaulted) throw finishedTask.Exception;

                await taskSemaphore.WaitAsync();
                // This Wait needs to be asynchronous since this could be the same thread that set tasksChanged.

                tasks.Remove(finishedTask);

                if (finishedTask == tasksChanged.Task)
                {
                    tasksChanged = new();
                    tasks.Add(tasksChanged.Task);
                }
            }

            taskSemaphore.Release();

            whenAllSemaphore.Release();
            whenAll.SetResult();
        }

        public async Task WhenAllCaptureException()
        {
            try
            {
                await WhenAll();
            }
            catch (AggregateException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException!).Throw();
            }
        }

        public void CopyTo(Task[] array, int arrayIndex)
        {
            taskSemaphore.Wait();
            try
            {
                foreach (Task task in tasks)
                if (task != tasksChanged.Task)
                    array[arrayIndex++] = task;
            }
            finally
            {
                taskSemaphore.Release();
            }
        }

        public void Dispose()
        {
            taskSemaphore.Dispose();
            whenAllSemaphore.Dispose();
        }

        public IEnumerator<Task> GetEnumerator() => tasks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
