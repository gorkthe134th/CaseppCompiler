using CaseppCompiler;
using CaseppCompiler.CodeOptimiser;
using CaseppCompiler.LexicalAnalyser.Tokens;

using NUnit.Framework.Constraints;

using System.Collections.Immutable;

namespace CaseppCompilerTest.TestClasses
{
    [TestFixture]
    internal class CodeOptimiserTests
    {
        private static readonly object[] tests =
        [
            new object[] { "riscv", "gp", "Uneffected" },
            new object[] { "riscv", "gp", "Main" },
            new object[] { "riscv", "gp", "LocalUneffected" },
            new object[] { "riscv", "gp", "GlobalFromFunction" },
            new object[] { "riscv", "gp", "GlobalFromChild" },
            new object[] { "riscv", "gp", "DataUneffected" },
        ];

        private static ImmutableDictionary<string, string> FolderNames = new Dictionary<string, string>()
        {
            ["riscv"] = "RISCVCodeOptimiserTests",
            ["gp"] = "GlobalVariableOptimiserTests",
        }.ToImmutableDictionary();

        [TestCaseSource(nameof(tests))]
        public async Task Test(string codeType, string optimiserType, string name)
        {
            ICodeOptimiser optimiser = RISCVCodeOptimiserFactory.Create(codeType, optimiserType);

            Stream<string> inputStream = new();
            Stream<string> outputStream = new();

            string path = $@"CodeOptimiserTests\{FolderNames[codeType]}\{FolderNames[optimiserType]}\{name}";

            foreach (var instruction in File.ReadAllLines(path + "In.asm")) await inputStream.AddAsync(instruction);
            inputStream.Complete();
            await optimiser.Analyse(inputStream, outputStream);

            var e = ((IEnumerable<string>)File.ReadAllLines(path + "Out.asm")).GetEnumerator();
            int line = 0;
            await foreach (var instruction in outputStream.GetAsyncEnumerable())
            {
                if (!e.MoveNext()) Assert.Fail($"Got more instructions than expected: {instruction}");
                Assert.That(instruction, Is.EqualTo(e.Current), $"Difference in line {++line}");
            }
            if (e.MoveNext()) Assert.Fail($"Got less instructions than expected.");
        }
    }
}
