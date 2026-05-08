using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

namespace CaseppCompiler.IntermediateProgramWriter
{
    public class FunctionWriterImplementation : IIntermediateProgramWriter
    {
        private record class WriteContext(StreamWriter Writer, int Line) : IDisposable
        {
            public int Line { get; set; } = Line;

            public void Dispose() => Writer.Dispose();
        }

        public Task Write(IntermediateProgram input, Stream ouput)
        {
            TaskCompletionSource taskCompletionSource = new();
            WriteContext context = new(new(ouput), 0);
            input.Functions.ItemAdded += (sender, function) => SubscribeToInstructions(function, context);
            input.Functions.Completed += (sender) =>
            {
                context.Dispose();
                taskCompletionSource.SetResult();
            };
            return taskCompletionSource.Task;
        }

        private static void SubscribeToInstructions(Function function, WriteContext context) =>
            function.Instructions.ItemTaken += (sender, instruction) => WriteInstruction(instruction, context);

        private static void WriteInstruction(Instruction instruction, WriteContext context)
        {
            var quad = instruction.ToQuad();
            int newLine = context.Line + 1;
            context.Writer.WriteLine($"{newLine}: {quad.Item1 ?? "_"}, {quad.Item2 ?? "_"}, {quad.Item3 ?? "_"}, {quad.Item4 ?? "_"}");
            context.Line = newLine;
        }
    }
}
