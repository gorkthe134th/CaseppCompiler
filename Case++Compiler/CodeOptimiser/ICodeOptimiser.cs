namespace CaseppCompiler.CodeOptimiser
{
    public interface ICodeOptimiser
    {
        public Task Analyse(Stream<string> input, Stream<string> output, CancellationToken? cancellationToken = null);
    }

    public static class RISCVCodeOptimiserFactory
    {
        public static ICodeOptimiser Create(string codeType = "", string optimiserType = "") =>
            codeType switch {
                "riscv" => optimiserType switch
                    {
                        "gp" => new RISCVCodeOptimiser.GlobalVariableOptimiser(),
                        _ => new RISCVCodeOptimiser.AggregateOptimiser(Create("riscv", "gp")),
                    },
                _ => Create("riscv", optimiserType),
            };
    }
}
