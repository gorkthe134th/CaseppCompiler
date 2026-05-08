namespace CaseppCompiler.CodeOptimiser.RISCVCodeOptimiser
{
    internal class GlobalVariableOptimiser : IRISCVCodeOptimiser
    {
        public async Task Analyse(Stream<string> input, Stream<string> output)
        {
            int? currentFunctionDepth = null;
            int? currentVariableSearchDepth = null;
            await foreach (string instruction in input.GetAsyncEnumerable())
            {
                string[] strings = instruction.Split(':', 2);
                if (strings.Length > 1)
                {
                    string label = strings[0];
                    strings = label.Split('_');
                    if (strings.Length > currentFunctionDepth) continue;
                    currentFunctionDepth = strings.Length;
                    continue;
                }
                switch (instruction)
                {
                    case "jr ra":
                        currentFunctionDepth = null;
                        continue;
                    case "mv t0, sp":
                        currentVariableSearchDepth = 1;
                        continue;
                    case "lw t0, 0(t0)":
                        currentVariableSearchDepth++;
                        continue;
                }
                if (currentFunctionDepth != currentVariableSearchDepth) continue;
                // TODO: Finish
            }
        }
    }
}
