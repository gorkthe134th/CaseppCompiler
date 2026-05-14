namespace CaseppCompiler.CodeOptimiser.RISCVCodeOptimiser
{
    internal class AggregateOptimiser(params IEnumerable<ICodeOptimiser> optimisers) : ICodeOptimiser
    {
        private ICodeOptimiser[] optimisers = [.. optimisers];

        public Task Analyse(Stream<string> input, Stream<string> output, CancellationToken? cancellationToken = null)
        {
            Stream<string>[] streams = [input, .. Enumerable.Repeat<Func<Stream<string>>>(() => new(), optimisers.Length - 1).Select(f => f()), output];
            return Task.WhenAll(optimisers.Select((o, i) => o.Analyse(streams[i], streams[i + 1])));
        }
    }
}
