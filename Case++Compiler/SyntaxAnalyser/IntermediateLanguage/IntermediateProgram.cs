using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Collections.Concurrent;
using System.Reflection;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    public class IntermediateProgram : IDisposable
    {
        internal BlockingCollection<Function> Functions { get; }

        private Function? currentFunction = null;
        private readonly Stack<object> currentVariables = [];
        private int nextTempIndex = 0;

        public IntermediateProgram(int? functionCapacity = null)
        {
            ConcurrentQueue<Function> queue = new();
            Functions = functionCapacity == null ? new(queue) : new(queue, (int)functionCapacity);
        }

        internal int CurrentLine { private get; set; } = 0;

        internal int CurrentColumn { private get; set; } = 0;

        internal void CreateFunction() => currentFunction = new Function(parent: currentFunction);

        internal Function CurrentFunction => currentFunction ?? throw new InvalidOperationException("No available functions.");

        internal void FinalizeFunction(Type finalInstructionType, IEnumerable<Type> finalInstructionParameterTypes, IEnumerable<object> finalInstructionParameters)
        {
            Function function = CurrentFunction;
            function.SetAllBreakTargets();
            AddInstruction(finalInstructionType, finalInstructionParameterTypes, finalInstructionParameters);
            Functions.Add(function);
            currentFunction = function.Parent;
        }

        internal void AddInstruction(Type type, IEnumerable<Type> parameterTypes, IEnumerable<object> parameters)
        {
            ConstructorInfo constructor = type.GetConstructor([typeof(int), typeof(int), ..parameterTypes])
                ?? throw new ArgumentException($"Cannot create {type} using parameter types \"{string.Concat(parameterTypes)}\"");
            CurrentFunction.AddInstruction((Instruction)constructor.Invoke([CurrentLine, CurrentColumn, ..parameters]));
        }

        internal void AddJumpInstructions(Type type, IEnumerable<Type> parameterTypes, IEnumerable<object> parameters, int start)
        {
            Function currentFunction = CurrentFunction;
            List<int> trueList = [currentFunction.CurrentPosition];
            AddInstruction(type, parameterTypes, parameters);
            List<int> falseList = [currentFunction.CurrentPosition];
            currentFunction.AddInstruction(new UnconditionalJumpInstruction(CurrentLine, CurrentColumn, null));
            currentVariables.Push(new JumpBlockInfo(trueList, falseList, start));
        }

        internal void AddBreakInstruction(uint count)
        {
            if (count == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddBreak(count);
            currentFunction.AddInstruction(new UnconditionalJumpInstruction(CurrentLine, CurrentColumn, null));
        }

        internal void AddRepeatInstruction(uint index)
        {
            if (index == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddInstruction(new UnconditionalJumpInstruction(CurrentLine, CurrentColumn, currentFunction.GetRepeatPoint(index)));
        }

        internal void PushVariable(object variable) => currentVariables.Push(variable);

        internal object PopVariable() => currentVariables.Pop();

        internal object PeekVariable() => currentVariables.Peek();

        internal string GenerateTemp() => $"_T{nextTempIndex++}";

        public IEnumerable<(string?, string?, string?, string?)> ToQuads()
        {
            int i = 0;
            return Functions.SelectMany(function =>
            {
                var ret = function.ToQuads(i);
                i += function.QuadCount;
                return ret;
            });
        }

        public void Dispose() => Functions.Dispose();
    }
}
