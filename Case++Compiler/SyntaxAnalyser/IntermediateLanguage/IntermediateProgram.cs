using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

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

            if (currentFunction != null)
                try
                {
                    currentFunction.AddSymbol(newFunction);
                }
                catch (ArgumentException e)
                {
                    throw new SyntaxAnalyserException(Position, $"Invalid Function \"{name}\" for the current Function.", e);
                }

            currentFunction = newFunction;
        }

        internal void AddFormalParameter(FormalParameter formalParameter)
        {
            try
            {
                CurrentFunction.AddParameter(formalParameter);
            }
            catch (ArgumentException e)
            {
                throw new SyntaxAnalyserException(Position, $"Invalid Formal Parameter \"{formalParameter}\" for Function \"{CurrentFunction.Name}\".", e);
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

        internal void SetLabel(string labelName) => AddSymbol(new Label(labelName, CurrentInstructionIndex));

        internal void EnterScope() => CurrentFunction.EnterScope();

        internal void ExitScope() => CurrentFunction.ExitScope();

        internal void AddSymbol(Symbol symbol)
        {
            try
            {
                CurrentFunction.AddSymbol(symbol);
            }
            catch (ArgumentException e)
            {
                throw new SyntaxAnalyserException(Position, $"Invalid {symbol.GetType().Name} \"{symbol.Name}\" for the current Function.", e);
            }
        }

        internal T GetAccessibleSymbol<T>(string name) where T : Symbol
        {
            Symbol symbol;

            try
            {
                symbol = CurrentFunction.GetSymbol(name);
            }
            catch (ArgumentException e)
            {
                throw new SyntaxAnalyserException(Position, $"Inaccessible {typeof(T).Name} \"{name}\" from the current Function.", e);
            }

            if (symbol is not T s) throw new SyntaxAnalyserException(Position, $"Symbol \"{name}\" is not a {typeof(T).Name}.");

            return s;
        }

        /// <summary>
        /// Gets the <see cref="Symbol"/> accessible from the current <see cref="Function"/> with the specified name, if it exists;
        /// otherwise, calls <paramref name="symbolFactory"/> and adds the result to the accessible <see cref="Symbol"/>s of the current <see cref="Function"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Symbol"/> to search.</param>
        /// <param name="symbolFactory">The method to call if the requested <see cref="Symbol"/> is not found.</param>
        /// <returns>The <see cref="Symbol"/> accessible from the current <see cref="Function"/> with the specified name or the result of calling <paramref name="symbolFactory"/>.</returns>
        internal Symbol GetOrAddAccessibleSymbol(string name, Func<Symbol> symbolFactory) => CurrentFunction.GetOrAddSymbol(name, symbolFactory);

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

        public void CompleteAdding()
        {
            Functions.CompleteAdding();
            Scopes.CompleteAdding();
        }

        public void Dispose()
        {
            Functions.Dispose();
            Scopes.Dispose();
        }
    }
}
