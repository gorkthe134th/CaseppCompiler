using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions;

using System.Reflection;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    public class IntermediateProgram
    {
        internal Function Main { get; }
        private readonly IList<Function> finalFunctions = [];

        private int currentLine = 0;
        private int currentColumn = 0;
        private readonly Stack<Function> currentFunctionStack;
        private readonly Stack<object> currentVariables = [];
        private int lastTemp = 0;

        public IntermediateProgram()
        {
            Main = new();
            currentFunctionStack = new([Main]);
        }

        internal void SetLineAndColumn(Token token)
        {
            currentLine = token.Line;
            currentColumn = token.Column;
        }

        internal void CreateFunction() => currentFunctionStack.Push(new());

        internal Function CurrentFunction => currentFunctionStack.Peek();

        internal void FinalizeFunction()
        {
            if (currentFunctionStack.Count <= 1) { finalFunctions.Add(currentFunctionStack.Pop()); return; }
            string functionName = string.Join('_', currentFunctionStack.Take(currentFunctionStack.Count - 1).Select(f => f.Name));
            Function function = currentFunctionStack.Pop();
            function.Name = functionName;
            finalFunctions.Add(function);
        }

        internal void AddInstruction(Type type, IEnumerable<Type> parameterTypes, IEnumerable<object> parameters)
        {
            ConstructorInfo constructor = type.GetConstructor([typeof(int), typeof(int), ..parameterTypes])
                ?? throw new ArgumentException($"Cannot create {type} using parameter types \"{string.Concat(parameterTypes)}\"");
            CurrentFunction.AddInstruction((Instruction)constructor.Invoke([currentLine, currentColumn, ..parameters]));
        }

        internal void AddJumpInstructions(Type type, IEnumerable<Type> parameterTypes, IEnumerable<object> parameters)
        {
            int start = CurrentFunction.CurrentPosition;
            List<int> trueList = [start];
            AddInstruction(type, parameterTypes, parameters);
            List<int> falseList = [CurrentFunction.CurrentPosition];
            CurrentFunction.AddInstruction(new UnconditionalJumpInstruction(currentLine, currentColumn));
            currentVariables.Push(new JumpBlockInfo(trueList, falseList, start));
        }

        internal void PushVariable(object variable) => currentVariables.Push(variable);

        internal object PopVariable() => currentVariables.Pop();

        internal string GenerateTemp() => $"_T{lastTemp++}";

        public void WriteToFile(StreamWriter writer)
        {
            int i = 0;
            foreach (var function in finalFunctions) function.WriteToFile(writer, ref i);
        }
    }
}
