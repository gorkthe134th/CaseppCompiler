using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    public class IntermediateProgram : IDisposable
    {
        internal BlockingCollection<Function> Functions { get; }

        private Function? currentFunction = null;
        private readonly Stack<object> compilerVariables = [];
        private int nextTempIndex = 0;

        public IntermediateProgram(int? functionCapacity = null)
        {
            ConcurrentQueue<Function> queue = new();
            Functions = functionCapacity == null ? new(queue) : new(queue, (int)functionCapacity);
        }

        internal int CurrentLine { private get; set; } = 0;

        internal int CurrentColumn { private get; set; } = 0;

        internal SyntaxAnalyserException CreateException(string message) => new($"{message}: Line {CurrentLine}, Column {CurrentColumn}");

        private Function CurrentFunction => currentFunction ?? throw new InvalidOperationException("No available functions.");

        internal int CurrentPosition => CurrentFunction.CurrentPosition;

        internal void CreateFunction(string name)
        {
            Function newFunction = new(name, currentFunction);
            FunctionSymbol functionSymbol = new(newFunction);
            if (currentFunction != null && !currentFunction.TryAddSymbol(functionSymbol))
                throw CreateException($"Symbol \"{name}\" already exists");
            compilerVariables.Push(functionSymbol);
            currentFunction = newFunction;
        }

        internal void FinalizeFunction<T>(Func<int, int, T> create) where T : Instruction
        {
            Function function = CurrentFunction;
            function.SetAllBreakTargets();
            CreateInstruction(create);
            Functions.Add(function);
            currentFunction = function.Parent;
        }

        internal T CreateInstruction<T>(Func<int, int, T> create) where T : Instruction
        {
            T instruction = create(CurrentLine, CurrentColumn);
            CurrentFunction.AddInstruction(instruction);
            return instruction;
        }

        internal void AddIntermediateLanguageInstruction(InstructionFactory.Argument arg0, InstructionFactory.Argument arg1, InstructionFactory.Argument arg2, InstructionFactory.Argument arg3) =>
            CurrentFunction.AddInstruction(InstructionFactory.Create(arg0, arg1, arg2, arg3, CurrentLine, CurrentColumn));

        internal void AddJumpInstructions<T>(Func<int, int, T> create, int start) where T : JumpInstruction
        {
            Function currentFunction = CurrentFunction;
            JumpInstruction trueJump = CreateInstruction(create);
            JumpInstruction falseJump = new UnconditionalJumpInstruction(CurrentLine, CurrentColumn, null);
            currentFunction.AddInstruction(falseJump);
            compilerVariables.Push(new JumpBlockInfo([trueJump], [falseJump], start));
        }

        internal void AddBreakInstruction(uint count)
        {
            if (count == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddBreak(new UnconditionalJumpInstruction(CurrentLine, CurrentColumn, null), count);
        }

        internal void SetBreakPoint() => CurrentFunction.SetBreakPoint();

        internal void AddRepeatInstruction(uint index)
        {
            if (index == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddInstruction(new UnconditionalJumpInstruction(CurrentLine, CurrentColumn, currentFunction.GetRepeatPoint(index)));
        }

        internal void SetRepeatPoint() => CurrentFunction.SetRepeatPoint();

        internal void SetLabel(string labelName)
        {
            if (!CurrentFunction.TryGetSymbol(labelName, out Symbol? symbol))
            {
                AddSymbol(new LabelSymbol(labelName, CurrentPosition));
                return;
            }
            if (symbol is not LabelSymbol label) throw CreateException($"Symbol \"{labelName}\" already exists");
            if (!label.TrySet(CurrentPosition)) throw CreateException($"Label \"{labelName}\" already exists");
        }

        internal void EnterScope() => CurrentFunction.EnterScope();

        internal void ExitScope() => CurrentFunction.ExitScope();

        internal void AddSymbol(Symbol symbol)
        {
            if (!CurrentFunction.TryAddSymbol(symbol)) throw CreateException($"Symbol \"{symbol.Name}\" already exists");
        }

        internal T GetSymbolInScope<T>(string name) where T : Symbol
        {
            if (!CurrentFunction.TryGetSymbol(name, out Symbol? symbol))
                throw CreateException($"Symbol \"{name}\" does not exists in the current scope");
            if (symbol is not T s)
                throw CreateException($"Symbol \"{name}\" is not a {typeof(T).Name.Replace("Symbol", null)}");
            return s;
        }

        internal bool TryGetSymbolInScope<T>(string name, [NotNullWhen(true)] out T? symbol) where T : Symbol
        {
            if (!CurrentFunction.TryGetSymbol(name, out Symbol? s) || s is not T ts)
            {
                symbol = null;
                return false;
            }
            symbol = ts;
            return true;
        }

        internal void InitialiseVariable(VariableSymbol variable) => CurrentFunction.InitialiseVariable(variable);

        internal void UseVariable(VariableSymbol variable)
        {
            if (!CurrentFunction.TryUseVariable(variable)) throw CreateException($"Use of uninitialised variable {variable.Name}");
        }
        internal void MergeVariableDependancies(FunctionSymbol calledFunction)
        {
            if (!CurrentFunction.MergeVariableDependancies(calledFunction.Function, out var uninitialisedVariables))
                throw CreateException($"Use of uninitialised variables in function \"{calledFunction.Function.Name}\": {string.Join(", ", uninitialisedVariables.Select(v => $"\"{v.Name}\""))}");
        }

        internal void PushCompilerVariable(object variable) => compilerVariables.Push(variable);

        internal T PopCompilerVariable<T>() => CastCompilerVariable<T>(compilerVariables.Pop());

        internal T PeekCompilerVariable<T>() => CastCompilerVariable<T>(compilerVariables.Peek());

        private static T CastCompilerVariable<T>(object variable) =>
            variable is T v ? v : throw new InvalidOperationException($"Variable \"{variable}\" is not compatible with type {typeof(T)}");

        internal void AddParameterToCallBlock(IActualParameter actualParameter)
        {
            FunctionCallBlockInfo callBlockInfo = PeekCompilerVariable<FunctionCallBlockInfo>();
            if (!callBlockInfo.TryAddParameter(actualParameter, out string? errorMessage))
            {
                if (errorMessage == null) throw CreateException($"Expected no more parameters for function \"{callBlockInfo.FunctionSymbol.Name}\"");
                else throw CreateException($"{errorMessage}: Function \"{callBlockInfo.FunctionSymbol.Name}\"");
            }
        }

        internal VariableSymbol GenerateTemp()
        {
            var temp = new VariableSymbol($"_T{nextTempIndex++}", false);
            AddSymbol(temp);
            return temp;
        }

        internal void CheckNoLeftoverVariables()
        {
            if (compilerVariables.Count != 0) throw new UnreachableException($"Leftover Variables: {string.Join(',', compilerVariables)}");
        }

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
