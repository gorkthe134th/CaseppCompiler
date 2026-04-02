using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Collections.Concurrent;
using System.Reflection;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    public class IntermediateProgram : IDisposable
    {
        internal BlockingCollection<Function> Functions { get; }

        private int currentLine = 0;
        private int currentColumn = 0;
        private readonly Stack<Function> currentFunctionStack = new();
        private readonly Stack<object> currentVariables = [];
        private int nextTempIndex = 0;

        public IntermediateProgram(int? functionCapacity = null)
        {
            ConcurrentQueue<Function> queue = new();
            Functions = functionCapacity == null ? new(queue) : new(queue, (int)functionCapacity);
        }

        internal void SetLineAndColumn(Token token)
        {
            currentLine = token.Line;
            currentColumn = token.Column;
        }

        internal void CreateFunction() => currentFunctionStack.Push(new());

        internal Function CurrentFunction => currentFunctionStack.Peek();

        internal void FinalizeFunction(Type finalInstructionType, IEnumerable<Type> finalInstructionParameterTypes, IEnumerable<object> finalInstructionParameters)
        {
            CurrentFunction.SetAllBreakTargets();
            AddInstruction(finalInstructionType, finalInstructionParameterTypes, finalInstructionParameters);

            if (currentFunctionStack.Count <= 1)
            {
                Functions.Add(currentFunctionStack.Pop());
                return;
            }

            string functionName = string.Join('_', currentFunctionStack.Reverse().Select(f => f.Name));
            Function function = currentFunctionStack.Pop();
            function.Name = functionName;

            Functions.Add(function);
        }

        internal void AddInstruction(Type type, IEnumerable<Type> parameterTypes, IEnumerable<object> parameters)
        {
            ConstructorInfo constructor = type.GetConstructor([typeof(int), typeof(int), ..parameterTypes])
                ?? throw new ArgumentException($"Cannot create {type} using parameter types \"{string.Concat(parameterTypes)}\"");
            CurrentFunction.AddInstruction((Instruction)constructor.Invoke([currentLine, currentColumn, ..parameters]));
        }

        internal void AddJumpInstructions(Type type, IEnumerable<Type> parameterTypes, IEnumerable<object> parameters, int start)
        {
            List<int> trueList = [CurrentFunction.CurrentPosition];
            AddInstruction(type, parameterTypes, parameters);
            List<int> falseList = [CurrentFunction.CurrentPosition];
            CurrentFunction.AddInstruction(new UnconditionalJumpInstruction(currentLine, currentColumn, null));
            currentVariables.Push(new JumpBlockInfo(trueList, falseList, start));
        }

        internal void AddBreakInstruction(uint count)
        {
            if (count == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddBreak(count);
            currentFunction.AddInstruction(new UnconditionalJumpInstruction(currentLine, currentColumn, null));
        }

        internal void AddRepeatInstruction(uint index)
        {
            if (index == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddInstruction(new UnconditionalJumpInstruction(currentLine, currentColumn, currentFunction.GetRepeatPoint(index)));
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
