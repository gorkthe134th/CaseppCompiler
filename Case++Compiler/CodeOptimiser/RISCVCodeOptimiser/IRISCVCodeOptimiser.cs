namespace CaseppCompiler.CodeOptimiser.RISCVCodeOptimiser
{
    public interface IRISCVCodeOptimiser
    {
        public Task Analyse(Stream<string> input, Stream<string> output, CancellationToken? cancellationToken = null);
    }

    public static class CodeOptimiserFactory
    {
        public static IRISCVCodeOptimiser Create(string type = "") =>
            type switch
            {
                "gp" => new GlobalVariableOptimiser(),
                _ => throw new InvalidOperationException("An Optimiser Type is required."),
            };
    }
}
