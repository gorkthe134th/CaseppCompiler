namespace CaseppCompiler.CodeOptimiser.RISCVCodeOptimiser
{
    internal class GlobalVariableOptimiser : ICodeOptimiser
    {
        public async Task Analyse(Stream<string> input, Stream<string> output, CancellationToken? cancellationToken = null)
        {
            int? currentFunctionDepth = null;
            int? currentVariableSearchDepth = null;
            bool active = true;
            bool addedInitialisation = false;

            await foreach (string instruction in input.GetAsyncEnumerable())
                if (!await HandleInstruction(instruction))
                    await output.AddAsync(instruction);

            output.Complete();

            async Task<bool> HandleInstruction(string instruction)
            {
                if (instruction == ".text") { active = true; return false; }
                if (instruction.StartsWith('.')) { active = false; return false; }
                if (!active) return false;

                if (!addedInitialisation)
                {
                    await output.AddAsync("mv gp, sp");
                    addedInitialisation = true;
                }

                string[] strings = instruction.Split(':', 2);
                if (strings.Length > 1)
                {
                    string label = strings[0];
                    strings = label.Split('_');
                    if (strings[0].Length > 0) currentFunctionDepth ??= strings.Length;
                    return false;
                }
                switch (instruction)
                {
                    case "jr ra":
                        currentFunctionDepth = null;
                        return false;
                    case "mv t0, sp":
                        currentVariableSearchDepth = 1;
                        return true;
                    case "lw t0, 0(t0)":
                        if (currentVariableSearchDepth != null)
                        {
                            currentVariableSearchDepth++;
                            return true;
                        }
                        return false;
                    default:
                        break;
                }
                if (!instruction.StartsWith("addi t0, t0, ") || currentFunctionDepth != currentVariableSearchDepth)
                {
                    if (currentVariableSearchDepth > 0) await output.AddAsync("mv t0, sp");
                    for (int i = 1; i < currentVariableSearchDepth; i++) await output.AddAsync("lw t0, 0(t0)");
                }
                else await output.AddAsync("mv t0, gp");
                currentVariableSearchDepth = null;
                return false;
            }
        }
    }
}
