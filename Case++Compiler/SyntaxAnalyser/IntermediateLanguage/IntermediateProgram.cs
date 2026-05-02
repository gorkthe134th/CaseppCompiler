using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage
{
    public class IntermediateProgram(int? functionCapacity = null, int? scopeCapacity = null) : IDisposable
    {
        internal Position Position { get; set; } = new(0, 0);

        internal BlockingCollection<Function> Functions { get; } =
            functionCapacity == null ? new(new ConcurrentQueue<Function>()) : new(new ConcurrentQueue<Function>(), (int)functionCapacity);

        internal BlockingCollection<Scope> Scopes { get; } =
            scopeCapacity == null ? new(new ConcurrentQueue<Scope>()) : new(new ConcurrentQueue<Scope>(), (int)scopeCapacity);

        private Function? currentFunction = null;
        private readonly Stack<object> compilerVariables = [];
        private int nextTempIndex = 0;

        private Function CurrentFunction => currentFunction ?? throw new InvalidOperationException("No available functions.");

        internal int CurrentInstructionIndex => CurrentFunction.CurrentInstructionIndex;

        internal void CreateFunction(string name, bool isMain = false)
        {
            Function newFunction = new(name, Scopes, currentFunction) { IsMain = isMain };

            if (currentFunction != null && !currentFunction.TryAddSymbol(newFunction))
                throw new SyntaxAnalyserException(Position, $"Symbol \"{name}\" already exists.");

            compilerVariables.Push(newFunction);
            currentFunction = newFunction;
        }

        internal void AddFormalParameter(FormalParameter formalParameter)
        {
            try
            {
                CurrentFunction.AddParameter(formalParameter);
            }
            catch (InvalidOperationException e)
            {
                throw new SyntaxAnalyserException(Position, $"Cannot add formal parameter \"{formalParameter}\" to function \"{CurrentFunction.Name}\".", e);
            }
        }

        internal void FinalizeFunction<T>(Func<Position, T> create) where T : Instruction
        {
            Function function = CurrentFunction;
            function.SetAllBreakTargets();
            CreateInstruction(create);
            Functions.Add(function);
            currentFunction = function.Parent;
        }

        internal T CreateInstruction<T>(Func<Position, T> create) where T : Instruction
        {
            T instruction = create(Position);
            CurrentFunction.AddInstruction(instruction);
            return instruction;
        }

        internal void AddIntermediateLanguageInstruction(InstructionFactory.Argument arg0, InstructionFactory.Argument arg1, InstructionFactory.Argument arg2, InstructionFactory.Argument arg3) =>
            CurrentFunction.AddInstruction(InstructionFactory.Create(arg0, arg1, arg2, arg3, Position));

        internal void AddJumpInstructions<T>(Func<Position, T> create, int start) where T : JumpInstruction
        {
            Function currentFunction = CurrentFunction;
            JumpInstruction trueJump = CreateInstruction(create);
            JumpInstruction falseJump = new UnconditionalJumpInstruction(Position);
            currentFunction.AddInstruction(falseJump);
            compilerVariables.Push(new JumpBlockInfo([trueJump], [falseJump], start));
        }

        internal void AddBreakInstruction(uint count)
        {
            if (count == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddBreak(new UnconditionalJumpInstruction(Position), count);
        }

        internal void SetBreakPoint() => CurrentFunction.SetBreakPoint();

        internal void AddRepeatInstruction(uint index)
        {
            if (index == 0) return;
            Function currentFunction = CurrentFunction;
            currentFunction.AddInstruction(new UnconditionalJumpInstruction(Position) { Target = currentFunction.GetRepeatPoint(index) });
        }

        internal void SetRepeatPoint() => CurrentFunction.SetRepeatPoint();

        internal void SetLabel(string labelName)
        {
            if (!CurrentFunction.TryGetSymbol(labelName, out Symbol? symbol))
            {
                AddSymbol(new Label(labelName, CurrentInstructionIndex));
                return;
            }
            if (symbol is not Label label) throw new SyntaxAnalyserException(Position, $"Symbol \"{labelName}\" already exists.");
            if (!label.TrySet(CurrentInstructionIndex)) throw new SyntaxAnalyserException(Position, $"Label \"{labelName}\" already exists.");
        }

        internal void EnterScope() => CurrentFunction.EnterScope();

        internal void ExitScope() => CurrentFunction.ExitScope();

        internal void AddSymbol(Symbol symbol)
        {
            if (!CurrentFunction.TryAddSymbol(symbol)) throw new SyntaxAnalyserException(Position, $"Symbol \"{symbol.Name}\" already exists.");
        }

        internal T GetSymbolInScope<T>(string name) where T : Symbol
        {
            if (!CurrentFunction.TryGetSymbol(name, out Symbol? symbol))
                throw new SyntaxAnalyserException(Position, $"Symbol \"{name}\" does not exists in the current scope.");
            if (symbol is not T s)
                throw new SyntaxAnalyserException(Position, $"Symbol \"{name}\" is not a {typeof(T).Name.Replace("Symbol", null)}.");
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

        internal void InitialiseVariable(Variable variable) => CurrentFunction.InitialiseVariable(variable);

        internal void UseVariable(Variable variable)
        {
            if (!CurrentFunction.TryUseVariable(variable)) throw new SyntaxAnalyserException(Position, $"Use of uninitialised variable {variable.Name}.");
        }
        internal void MergeVariableDependancies(Function calledFunction)
        {
            if (!CurrentFunction.MergeVariableDependancies(calledFunction, out var uninitialisedVariables))
                throw new SyntaxAnalyserException(Position, $"Use of uninitialised variables in function \"{calledFunction.Name}\": {string.Join(", ", uninitialisedVariables.Select(v => $"\"{v.Name}\""))}.");
        }

        internal void PushCompilerVariable(object variable) => compilerVariables.Push(variable);

        internal T PopCompilerVariable<T>() => CastCompilerVariable<T>(compilerVariables.Pop());

        internal T PeekCompilerVariable<T>() => CastCompilerVariable<T>(compilerVariables.Peek());

        private static T CastCompilerVariable<T>(object variable) =>
            variable is T v ? v : throw new InvalidOperationException($"Variable \"{variable}\" is not compatible with type {typeof(T)}.");

        internal Variable GenerateTemp()
        {
            var temp = new Variable($"_T{nextTempIndex++}", false);
            AddSymbol(temp);
            return temp;
        }

        internal void CheckNoLeftoverVariables()
        {
            if (compilerVariables.Count != 0) throw new UnreachableException($"Leftover Variables: {string.Join(',', compilerVariables)}.");
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
