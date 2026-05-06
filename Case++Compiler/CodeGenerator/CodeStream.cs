using System.Collections.Concurrent;

namespace CaseppCompiler.CodeGenerator
{
    public class CodeStream : IDisposable
    {
        private readonly BlockingCollection<string> code;
        private readonly CancellationToken? cancellationToken;

        public CodeStream(int? capacity = null, CancellationToken? cancellationToken = null)
        {
            this.cancellationToken = cancellationToken;
            ConcurrentQueue<string> queque = new();
            code = capacity == null ? new(queque) : new(queque, boundedCapacity: (int)capacity);
        }

        public void Add(string instruction)
        {
            if (cancellationToken == null) code.Add(instruction);
            else code.Add(instruction, (CancellationToken)cancellationToken);
        }

        public string Take() =>
            cancellationToken != null
            ? code.Take((CancellationToken)cancellationToken)
            : code.Take();

        public IEnumerable<string> GetConsumingEnumerable() =>
            cancellationToken != null
            ? code.GetConsumingEnumerable((CancellationToken)cancellationToken)
            : code.GetConsumingEnumerable();

        public void CompleteAdding() => code.CompleteAdding();

        public void Dispose() => code.Dispose();
    }
}
