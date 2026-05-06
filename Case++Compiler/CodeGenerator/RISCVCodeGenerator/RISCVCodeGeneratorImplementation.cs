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

        public void Analyse(IntermediateProgram input, CodeStream? output = null)
        {
            try
            {
                ConcurrentDictionary<Function, FunctionInfo> functionInfos = new(2, 3);
                Task.Run(() =>
                {
                    foreach (var scope in input.Scopes.GetConsumingEnumerable())
                    {
                        FunctionInfo info = functionInfos.GetOrAdd(scope.EncompassingFunction, f => new(new(), []));
                        info.AddStackFrameFromScope(scope);
                    }
                });

                output?.Add($"j _main");

                foreach (Function function in input.Functions.GetConsumingEnumerable())
                {
                    FunctionInfo functionInfo = functionInfos.GetOrAdd(function, f => new(new(), []));
                    functionInfo.Ready.Task.Wait();

                    if (function.IsMain) output?.Add($"_main:");
                    output?.Add($"{function.FullName}:");
                    output?.Add($"sw ra, 4(sp)");

                    List<ParameterInstruction> nextCallParameters = [];
                    List<StackFrame> loadedFrames = functionInfo.StackFrames.Count > 0 ? [functionInfo.StackFrames[0]] : [];
                    int currentInstructionIndex = 0;
                    foreach (var instruction in function.Instructions)
                    {
                        output?.Add($"{function.FullName}_{currentInstructionIndex}:");

                        loadedFrames.RemoveAll(sf => sf.End <= currentInstructionIndex);
                        List<StackFrame> frames = functionInfo.StackFrames;
                        for (int i = frames.Count - 1; i >= 0; i--)
                            if (frames[i].Start == currentInstructionIndex)
                                loadedFrames.Add(frames[i]);

                        switch (instruction)
                        {
                            case AssignmentInstruction assignmentInstruction:
                                GenerateValueLoadingCode(assignmentInstruction.Value, "t1", instruction.Position);
                                GenerateVariableStoringCode(assignmentInstruction.Variable, "t1", instruction.Position);
                                break;
                            case OperationInstruction operationInstruction:
                                GenerateValueLoadingCode(operationInstruction.Operand1, "t1", instruction.Position);
                                GenerateValueLoadingCode(operationInstruction.Operand2, "t2", instruction.Position);
                                output?.Add($"{operationMap[operationInstruction.Operation]} t1, t1, t2");
                                GenerateVariableStoringCode(operationInstruction.Result, "t1", instruction.Position);
                                break;
                            case InputInstruction inputInstruction:
                                output?.Add($"li a7, 5");
                                output?.Add($"ecall");
                                GenerateVariableStoringCode(inputInstruction.Variable, "a0", instruction.Position);
                                break;
                            case OutputInstruction outputInstruction:
                                GenerateValueLoadingCode(outputInstruction.Value, "a0", instruction.Position);
                                output?.Add($"li a7, 1");
                                output?.Add($"ecall");
                                break;
                            case HaltInstruction haltInstruction:
                                output?.Add($"li a0, 0");
                                output?.Add($"li a7, 93");
                                output?.Add($"ecall");
                                break;
                            case ParameterInstruction parameterInstruction:
                                nextCallParameters.Add(parameterInstruction);
                                break;
                                // "par" does not produce code in this implementation.
                                // Warning: This does not allow coditional parameter passing.
                            case CallInstruction callInstruction:
                                {
                                    Function callee = callInstruction.Function;

                                    if (!functionInfos.TryGetValue(callee, out FunctionInfo? calleeList))
                                        throw new CodeGeneratorException(callInstruction.Position, $"Call of undefined function \"{callee.Name}\".");

                                    if (calleeList.StackFrames.Count > 0)
                                    {
                                        StackFrame baseFrame = calleeList.StackFrames[^1];

                                        output?.Add($"addi fp, sp, {loadedFrames[^1].SkipOffset}");

                                        output?.Add($"mv t0, sp");
                                        Function functionReached = function;
                                        while (callee.Parent != functionReached)
                                        {
                                            output?.Add($"lw t0, 0(t0)");
                                            functionReached = functionReached.Parent ??
                                                throw new CodeGeneratorException(callInstruction.Position, $"Inaccessible Parent Function \"{callee.Parent?.Name}\" from Function \"{function.Name}\".");
                                        }
                                        output?.Add($"sw t0, 0(fp)");
                                        
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

                                            switch (actualParameter)
                                            {
                                                case InParameter inParameter:
                                                    GenerateValueLoadingCode(inParameter.Value, "t1", instruction.Position);
                                                    break;
                                                case InOutParameter inOutParameter:
                                                    GenerateVariableFetchingCode(inOutParameter.Variable, instruction.Position);
                                                    output?.Add($"mv t1, t0");
                                                    break;
                                                case OutParameter outParameter:
                                                    GenerateVariableFetchingCode(outParameter.Variable, instruction.Position);
                                                    output?.Add($"mv t1, t0");
                                                    break;
                                                default:
                                                    throw new ArgumentException($"Invalid parameter: {actualParameter}");
                                            }
                                            output?.Add($"sw t1, {baseFrame.GetOffset(formalParameter.AssociatedVariable)}(fp)");
                                        }
                                        output?.Add($"mv sp, fp");
                                        nextCallParameters = [];
                                    }
                                    output?.Add($"jal ra, {callee.FullName}");
                                }
                                break;
                            case ReturnInstruction returnInstruction:
                                switch ((function.ReturnVariable == null, returnInstruction.Value == null))
                                {
                                    case (false, false):
                                        GenerateValueLoadingCode(returnInstruction.Value!, "t1", instruction.Position);
                                        GenerateVariableStoringCode(function.ReturnVariable!, "t1", instruction.Position);
                                        break;
                                    case (true, true):
                                        break;
                                    case (false, true):
                                        throw new CodeGeneratorException(returnInstruction.Position, $"Function \"{function.Name}\" must return a value.");
                                    case (true, false):
                                        throw new CodeGeneratorException(returnInstruction.Position, $"Procedure \"{function.Name}\" cannot return a value.");
                                }
                                output?.Add($"lw ra, 4(sp)");
                                output?.Add($"jr ra");
                                break;
                            case UnconditionalJumpInstruction unconditionalJumpInstruction:
                                output?.Add($"j {function.FullName}_{unconditionalJumpInstruction.Target}");
                                break;
                            case ComparisonJumpInstruction comparisonJumpInstruction:
                                GenerateValueLoadingCode(comparisonJumpInstruction.Operand1, "t1", instruction.Position);
                                GenerateValueLoadingCode(comparisonJumpInstruction.Operand2, "t2", instruction.Position);
                                output?.Add($"{comparisonMap[comparisonJumpInstruction.Operation]} t1, t2, {function.FullName}_{comparisonJumpInstruction.Target}");
                                break;
                            default:
                                throw new ArgumentException($"Invalid Intermediate Language Instruction: {instruction}");
                        }
                        currentInstructionIndex++;
                    }

                    void GenerateValueLoadingCode(Value value, string register, Position position)
                    {
                        if (value.IsConstant(out var constant))
                            output?.Add($"li {register}, {constant}");
                        else if (value.IsVariable(out var variable))
                            GenerateVariableLoadingCode(variable, register, position);
                    }

                    void GenerateVariableLoadingCode(Variable variable, string register, Position position)
                    {
                        GenerateVariableFetchingCode(variable, position);
                        output?.Add($"lw {register}, 0(t0)");
                    }

                    void GenerateVariableStoringCode(Variable variable, string register, Position position)
                    {
                        GenerateVariableFetchingCode(variable, position);
                        output?.Add($"sw {register}, 0(t0)");
                    }

                    /// <summary>
                    /// Generates a block of code that locates the specified variable in the stack.
                    /// After the generated code is executed, t0 holds the location of the data held by the variable.
                    /// If the specified variable is a reference variable, the reference itself is returned.
                    /// The code uses only the register t0.
                    /// </summary>
                    /// <param name="variable">The variable to locate.</param>
                    /// <param name="output">The collection to store the generated code.</param>
                    void GenerateVariableFetchingCode(Variable variable, Position position)
                    {
                        output?.Add($"mv t0, sp");
                        List<StackFrame> currentFrames = loadedFrames;
                        int variableOffset = 0;
                        while (!currentFrames.Any(f =>
                        {
                            if (!f.TryGetOffset(variable, out int? offset)) return false;
                            variableOffset = (int)offset;
                            return true;
                        }))
                        {
                            Function parent = function.Parent ??
                                throw new CodeGeneratorException(position, $"Inaccessible Variable \"{variable.Name}\" from Function \"{function.Name}\".");
                            currentFrames = functionInfos[parent].StackFrames;
                            output?.Add($"lw t0, 0(t0)");
                        }
                        output?.Add($"addi t0, t0, {variableOffset}");
                        if (variable.IsReference) output?.Add($"lw t0, 0(t0)");
                    }
                }
            }
            finally
            {
                output?.CompleteAdding();
            }
        }
    }
}
