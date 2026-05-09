using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

namespace CaseppCompiler.Writers.IntermediateProgramWriter
{
    public class FunctionWriterImplementation : IIntermediateProgramWriter
    {
        private record class FunctionWriteContext(StreamWriter Writer, int Line) : WriteContext(Writer)
        {
            public int Line { get; set; } = Line;

            public HashSet<Function> CompletedFunctions { get; } = [];
        }

        public Task Write(IntermediateProgram input, Stream ouput)
        {
            FunctionWriteContext context = new(new(ouput), 0);
            input.Functions.ItemAdded += (sender, function) => SubscribeToInstructions(function, context);
            input.Functions.Completed += (sender) => context.AllowDispose();
            return context.WriteComplete;
        }

        private static void SubscribeToInstructions(Function function, FunctionWriteContext context)
        {
            context.AddUser();
            function.Instructions.ItemTaken += (sender, instruction) =>
            {
                WriteInstruction(instruction, context);
                lock (context)
                {
                    if (function.Instructions.Count == 0 && context.CompletedFunctions.Contains(function))
                    {
                        context.CompletedFunctions.Remove(function);
                        context.RemoveUser();
                    }
                }
            };
            function.Instructions.Completed += (sender) =>
            {
                lock (context)
                {
                    if (function.Instructions.Count == 0)
                    {
                        context.RemoveUser();
                        return;
                    }
                    context.CompletedFunctions.Add(function);
                }
            };
        }

        private static void WriteInstruction(Instruction instruction, FunctionWriteContext context)
        {
            var quad = instruction.ToQuad();
            int newLine = context.Line + 1;
            context.Writer.WriteLine($"{newLine}: {quad.Item1 ?? "_"}, {quad.Item2 ?? "_"}, {quad.Item3 ?? "_"}, {quad.Item4 ?? "_"}");
            context.Line = newLine;
        }
    }
}
