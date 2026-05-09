using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace CaseppCompiler.CodeGenerator.RISCVCodeGenerator
{
    internal class RISCVCodeGeneratorImplementation : ICodeGenerator
    {
        private static readonly ImmutableDictionary<OperationType, string> operationMap = new Dictionary<OperationType, string>()
        {
            [OperationType.Add] = "add",
            [OperationType.Subtract] = "sub",
            [OperationType.Multiply] = "mul",
            [OperationType.Divide] = "div",
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<OperationType, string> comparisonMap = new Dictionary<OperationType, string>()
        {
            [OperationType.EqualTo] = "beq",
            [OperationType.LessThan] = "blt",
            [OperationType.GreaterThan] = "bgt",
            [OperationType.NotEqualTo] = "bne",
            [OperationType.LessThanOrEqualTo] = "ble",
            [OperationType.GreaterThanOrEqualTo] = "bge",
        }.ToImmutableDictionary();

        public async Task Analyse(IntermediateProgram input, Stream<string>? output = null)
        {
            try
            {
                if (output != null) await output.AddAsync($"j _main");

                ConcurrentDictionary<Function, FunctionInfo> functionInfos = [];
                SemaphoreSlim functionSemaphore = new(1, 1);
                TaskCompletionSource taskAdded = new();
                TaskCompletionSource? scopesReady = null;
                TaskCompletionSource? scopesReadyRequest = new();
                List<Task> tasks = [taskAdded.Task];

                Monitor.Enter(tasks);

                tasks.Add(ParseFunctions());
                tasks.Add(ParseScopes());

                while (tasks.Count > 1)
                {
                    Task<Task> anyTaskComplete;
                    anyTaskComplete = Task.WhenAny(tasks);

                    Monitor.Exit(tasks);

                    Task finishedTask = await anyTaskComplete;
                    if (finishedTask.IsFaulted) throw finishedTask.Exception.InnerException!;
                    // There is no need to rethrow the whole AggregateException object.
                    // The tasks in the list originate from async methods (and a TaskCompletionSource).
                    // Only one exception is expected to be thrown per task.

                    Monitor.Enter(tasks);

                    tasks.Remove(finishedTask);

                    if (finishedTask == taskAdded.Task)
                    {
                        taskAdded = new();
                        tasks.Add(taskAdded.Task);
                    }
                }

                Monitor.Exit(tasks);


                void AddTask(Task task)
                {
                    lock (tasks)
                    {
                        tasks.Add(task);
                        taskAdded.TrySetResult();
                    }
                }

                async Task ParseFunctions()
                {
                    await foreach (Function function in input.Functions.GetAsyncEnumerable())
                        AddTask(ParseFunction(function));
                }

                async Task ParseScopes()
                {
                    var e = input.Scopes.GetAsyncEnumerable().GetAsyncEnumerator();
                    while (true)
                    {
                        ValueTask<bool> next = e.MoveNextAsync();
                        if (!next.IsCompleted)
                        {
                            Task<bool> nextTask = next.AsTask();
                            while (true)
                            {
                                Task finished = await Task.WhenAny(nextTask, scopesReadyRequest.Task);
                                if (finished != scopesReadyRequest.Task || nextTask.IsCompleted) break;
                                scopesReadyRequest = new();
                                scopesReady?.SetResult();
                            }
                        }
                        if (next.Result == false) break;
                        Scope scope = e.Current;
                        FunctionInfo info = functionInfos.GetOrAdd(scope.EncompassingFunction, info = new());
                        info.AddStackFrameFromScope(scope);
                    }
                    scopesReadyRequest = null;
                    scopesReady?.TrySetResult();
                }

                async Task ParseFunction(Function function)
                {
                    try
                    {
                        FunctionInfo functionInfo = functionInfos.GetOrAdd(function, functionInfo = new());
                        List<ParameterInstruction> nextCallParameters = [];
                        List<StackFrame> loadedFrames = functionInfo.StackFrames.Count > 0 ? [functionInfo.StackFrames[0]] : [];
                        int currentInstructionIndex = 0;
                        bool addedHeader = false;
                        await foreach (var instruction in function.Instructions.GetAsyncEnumerable(i => i.Complete))
                        {
                            if (output != null)
                            {
                                if (!addedHeader)
                                {
                                    await functionSemaphore.WaitAsync(); // Only allow one thread to produce code at a time.
                                    if (function.IsMain) await output.AddAsync($"_main:");
                                    await output.AddAsync($"{function.FullName}:");
                                    await output.AddAsync($"sw ra, 4(sp)");
                                    addedHeader = true;
                                }

                                await output.AddAsync($"{function.FullName}_{currentInstructionIndex}:");
                            }

                            if (scopesReadyRequest != null)
                            {
                                scopesReady = new();
                                scopesReadyRequest.SetResult();
                                await scopesReady.Task;
                            }

                            loadedFrames.RemoveAll(sf => sf.End <= currentInstructionIndex);
                            List<StackFrame> frames = functionInfo.StackFrames;
                            for (int i = 0; i < frames.Count; i++)
                                if (frames[i].Start == currentInstructionIndex)
                                    loadedFrames.Add(frames[i]);

                            await ParseInstruction(instruction);

                            currentInstructionIndex++;
                        }

                        async Task ParseInstruction(Instruction instruction)
                        {
                            switch (instruction)
                            {
                                case AssignmentInstruction assignmentInstruction:
                                    if (output != null)
                                    {
                                        await GenerateValueLoadingCode(assignmentInstruction.Value, "t1", instruction.Position);
                                        await GenerateVariableStoringCode(assignmentInstruction.Variable, "t1", instruction.Position);
                                    }
                                    break;
                                case OperationInstruction operationInstruction:
                                    if (output != null)
                                    {
                                        await GenerateValueLoadingCode(operationInstruction.Operand1, "t1", instruction.Position);
                                        await GenerateValueLoadingCode(operationInstruction.Operand2, "t2", instruction.Position);
                                        await output.AddAsync($"{operationMap[operationInstruction.Operation]} t1, t1, t2");
                                        await GenerateVariableStoringCode(operationInstruction.Result, "t1", instruction.Position);
                                    }
                                    break;
                                case InputInstruction inputInstruction:
                                    if (output != null)
                                    {
                                        await output.AddAsync($"li a7, 5");
                                        await output.AddAsync($"ecall");
                                        await GenerateVariableStoringCode(inputInstruction.Variable, "a0", instruction.Position);
                                    }
                                    break;
                                case OutputInstruction outputInstruction:
                                    if (output != null)
                                    {
                                        await GenerateValueLoadingCode(outputInstruction.Value, "a0", instruction.Position);
                                        await output.AddAsync($"li a7, 1");
                                        await output.AddAsync($"ecall");
                                    }
                                    break;
                                case HaltInstruction haltInstruction:
                                    if (output != null)
                                    {
                                        await output.AddAsync($"li a0, 0");
                                        await output.AddAsync($"li a7, 93");
                                        await output.AddAsync($"ecall");
                                    }
                                    break;
                                case ParameterInstruction parameterInstruction:
                                    nextCallParameters.Add(parameterInstruction);
                                    break;
                                // "par" does not produce code in this implementation.
                                // Warning: This does not allow coditional parameter passing.
                                case CallInstruction callInstruction:
                                    {
                                        Function callee = callInstruction.Function;

                                        if (!functionInfos.TryGetValue(callee, out FunctionInfo? calleeInfo))
                                            throw new CodeGeneratorException(callInstruction.Position, $"Call of undefined function \"{callee.Name}\".");

                                        int bytesSkippedInStack = 0;
                                        if (calleeInfo.StackFrames.Count > 0)
                                        {
                                            StackFrame baseFrame = calleeInfo.StackFrames[0];

                                            if (output != null)
                                            {
                                                await output.AddAsync($"addi fp, sp, {bytesSkippedInStack = loadedFrames[^1].SkipOffset}");

                                                await output.AddAsync($"mv t0, sp");
                                                Function functionReached = function;
                                                while (callee.Parent != functionReached)
                                                {
                                                    await output.AddAsync($"lw t0, 0(t0)");
                                                    functionReached = functionReached.Parent ??
                                                        throw new CodeGeneratorException(callInstruction.Position, $"Inaccessible Parent Function \"{callee.Parent?.Name}\" from Function \"{function.Name}\".");
                                                }
                                                await output.AddAsync($"sw t0, 0(fp)");
                                            }

                                            if (nextCallParameters.Count != callee.FormalParameters.Count)
                                                throw new CodeGeneratorException(callInstruction.Position,
                                                    $"Function \"{callee.Name}\" requires {callee.FormalParameters.Count} parameters, but got {nextCallParameters.Count}.");

                                            foreach ((var parameterInstruction, var formalParameter) in nextCallParameters.Zip(callee.FormalParameters))
                                            {
                                                var actualParameter = parameterInstruction.Parameter;

                                                try
                                                {
                                                    formalParameter.Match(actualParameter);
                                                }
                                                catch (FormalParameter.MismatchException e)
                                                {
                                                    throw new CodeGeneratorException(parameterInstruction.Position,
                                                        $"Function \"{callee.Name}\" actual parameter \"{actualParameter}\" does not match formal parameter \"{formalParameter}\".", e);
                                                }

                                                if (output != null)
                                                {
                                                    switch (actualParameter)
                                                    {
                                                        case InParameter inParameter:
                                                            await GenerateValueLoadingCode(inParameter.Value, "t1", instruction.Position);
                                                            break;
                                                        case InOutParameter inOutParameter:
                                                            await GenerateVariableFetchingCode(inOutParameter.Variable, instruction.Position);
                                                            await output.AddAsync($"mv t1, t0");
                                                            break;
                                                        case OutParameter outParameter:
                                                            await GenerateVariableFetchingCode(outParameter.Variable, instruction.Position);
                                                            await output.AddAsync($"mv t1, t0");
                                                            break;
                                                        default:
                                                            throw new ArgumentException($"Invalid parameter: {actualParameter}");
                                                    }
                                                    await output.AddAsync($"sw t1, {baseFrame.GetOffset(formalParameter.AssociatedVariable)}(fp)");
                                                }
                                            }
                                            if (output != null) await output.AddAsync($"mv sp, fp");
                                            nextCallParameters = [];
                                        }
                                        if (output != null)
                                        {
                                            await output.AddAsync($"jal ra, {callee.FullName}");
                                            if (bytesSkippedInStack > 0)
                                                await output.AddAsync($"addi sp, sp, -{bytesSkippedInStack}");
                                        }
                                    }
                                    break;
                                case ReturnInstruction returnInstruction:
                                    switch ((function.ReturnVariable == null, returnInstruction.Value == null))
                                    {
                                        case (false, false):
                                            if (output != null)
                                            {
                                                await GenerateValueLoadingCode(returnInstruction.Value!, "t1", instruction.Position);
                                                await GenerateVariableStoringCode(function.ReturnVariable!, "t1", instruction.Position);
                                            }
                                            break;
                                        case (true, true):
                                            break;
                                        case (false, true):
                                            throw new CodeGeneratorException(returnInstruction.Position, $"Function \"{function.Name}\" must return a value.");
                                        case (true, false):
                                            throw new CodeGeneratorException(returnInstruction.Position, $"Procedure \"{function.Name}\" cannot return a value.");
                                    }
                                    if (output != null)
                                    {
                                        await output.AddAsync($"lw ra, 4(sp)");
                                        await output.AddAsync($"jr ra");
                                    }
                                    break;
                                case UnconditionalJumpInstruction unconditionalJumpInstruction:
                                    if (output != null) await output.AddAsync($"j {function.FullName}_{unconditionalJumpInstruction.Target}");
                                    break;
                                case ComparisonJumpInstruction comparisonJumpInstruction:
                                    if (output != null)
                                    {
                                        await GenerateValueLoadingCode(comparisonJumpInstruction.Operand1, "t1", instruction.Position);
                                        await GenerateValueLoadingCode(comparisonJumpInstruction.Operand2, "t2", instruction.Position);
                                        await output.AddAsync($"{comparisonMap[comparisonJumpInstruction.Operation]} t1, t2, {function.FullName}_{comparisonJumpInstruction.Target}");
                                    }
                                    break;
                                default:
                                    throw new ArgumentException($"Invalid Intermediate Language Instruction: {instruction}");
                            }
                        }

                        async Task GenerateValueLoadingCode(Value value, string register, Position position)
                        {
                            if (value.IsConstant(out var constant))
                                await output.AddAsync($"li {register}, {constant}");
                            else if (value.IsVariable(out var variable))
                                await GenerateVariableLoadingCode(variable, register, position);
                        }

                        async Task GenerateVariableLoadingCode(Variable variable, string register, Position position)
                        {
                            await GenerateVariableFetchingCode(variable, position);
                            await output.AddAsync($"lw {register}, 0(t0)");
                        }

                        async Task GenerateVariableStoringCode(Variable variable, string register, Position position)
                        {
                            await GenerateVariableFetchingCode(variable, position);
                            await output.AddAsync($"sw {register}, 0(t0)");
                        }

                        /// <summary>
                        /// Generates a block of code that locates the specified variable in the stack.
                        /// After the generated code is executed, t0 holds the location of the data held by the variable.
                        /// If the specified variable is a reference variable, the reference itself is returned.
                        /// The code uses only the register t0.
                        /// </summary>
                        /// <param name="variable">The variable to locate.</param>
                        /// <param name="output">The collection to store the generated code.</param>
                        async Task GenerateVariableFetchingCode(Variable variable, Position position)
                        {
                            await output.AddAsync($"mv t0, sp");
                            Function functionReached = function;
                            int variableOffset = 0;
                            while (!functionInfos[functionReached].StackFrames.Any(f =>
                            {
                                if (!f.TryGetOffset(variable, out int? offset)) return false;
                                variableOffset = (int)offset;
                                return true;
                            }))
                            {
                                await output.AddAsync($"lw t0, 0(t0)");
                                functionReached = functionReached.Parent ??
                                    throw new CodeGeneratorException(position, $"Inaccessible Variable \"{variable.Name}\" from Function \"{function.Name}\".");
                            }
                            await output.AddAsync($"addi t0, t0, {variableOffset}");
                            if (variable.IsReference) await output.AddAsync($"lw t0, 0(t0)");
                        }
                    }
                    finally
                    {
                        functionSemaphore.Release();
                    }
                }
            }
            finally
            {
                output?.Complete();
            }
        }
    }
}
