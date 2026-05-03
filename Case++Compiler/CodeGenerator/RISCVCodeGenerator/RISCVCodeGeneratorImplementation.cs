using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using Microsoft.Win32;

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

        private record StackFrameList(TaskCompletionSource Complete, List<StackFrame> StackFrames);

        public void Analyse(IntermediateProgram input, BlockingCollection<string>? output = null)
        {
            try
            {
                ConcurrentDictionary<Function, StackFrameList> stackFrames = new(2, 3);
                Task.Run(() =>
                {
                    foreach (var scope in input.Scopes.GetConsumingEnumerable())
                    {
                        StackFrameList list = stackFrames.GetOrAdd(scope.EncompassingFunction, f => new(new(), []));
                        if (list.Complete.Task.IsCompleted)
                            throw new ArgumentException($"Function \"{scope.EncompassingFunction}\" has been finalized, but got unprocessed scope for it: {scope}");
                        list.StackFrames.Add(new(from symbol in scope.Symbols
                                                 let variable = symbol as Variable
                                                 where variable != null
                                                 select variable));
                        if (scope.IsBase) list.Complete.SetResult();
                    }
                });

                output?.Add($"j main");

                foreach (Function function in input.Functions.GetConsumingEnumerable())
                {
                    StackFrameList stackFrameList = stackFrames.GetOrAdd(function, f => new(new(), []));
                    stackFrameList.Complete.Task.Wait();

                    if (function.IsMain) output?.Add($"main:");
                    output?.Add($"{function.FullName}:");

                    IList<ParameterInstruction> nextCallParameters = [];
                    int currentIndex = 0;
                    foreach (var instruction in function.Instructions)
                    {
                        output?.Add($"l{currentIndex}:");
                        switch (instruction)
                        {
                            case AssignmentInstruction assignmentInstruction:
                                GenerateValueLoadingCode(assignmentInstruction.Value, "t1");
                                GenerateVariableStoringCode(assignmentInstruction.Variable, "t1");
                                break;
                            case OperationInstruction operationInstruction:
                                GenerateValueLoadingCode(operationInstruction.Operand1, "t1");
                                GenerateValueLoadingCode(operationInstruction.Operand2, "t2");
                                output?.Add($"{operationMap[operationInstruction.Operation]} t1, t1, t2");
                                GenerateVariableStoringCode(operationInstruction.Result, "t1");
                                break;
                            case InputInstruction inputInstruction:
                                output?.Add($"li a7, 5");
                                output?.Add($"ecall");
                                GenerateVariableStoringCode(inputInstruction.Variable, "a0");
                                break;
                            case OutputInstruction outputInstruction:
                                GenerateValueLoadingCode(outputInstruction.Value, "a0");
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
                            case CallInstruction callInstruction:
                                {
                                    Function callee = callInstruction.Function;

                                    if (!stackFrames.TryGetValue(callee, out StackFrameList? calleeList))
                                        throw new CodeGeneratorException(callInstruction.Position, $"Call of undefined function \"{callee.Name}\"");

                                    if (calleeList.StackFrames.Count > 0)
                                    {
                                        StackFrame firstFrame = calleeList.StackFrames[0];

                                        output?.Add($"addi fp, sp, {firstFrame.Length}");

                                        if (nextCallParameters.Count != callee.FormalParameters.Count)
                                            throw new CodeGeneratorException(callInstruction.Position,
                                                $"Function \"{callee.Name}\" requires {callee.FormalParameters.Count} parameters, but got {nextCallParameters.Count} parameters");

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
                                                    $"Function \"{callee.Name}\" actual parameter \"{actualParameter}\" does not match formal parameter \"{formalParameter}\"", e);
                                            }

                                            switch (actualParameter)
                                            {
                                                case InParameter inParameter:
                                                    GenerateValueLoadingCode(inParameter.Value, "t1");
                                                    break;
                                                case InOutParameter inOutParameter:
                                                    GenerateVariableFetchingCode(inOutParameter.Variable);
                                                    output?.Add($"mv t1, t0");
                                                    break;
                                                case OutParameter outParameter:
                                                    GenerateVariableFetchingCode(outParameter.Variable);
                                                    output?.Add($"mv t1, t0");
                                                    break;
                                                default:
                                                    throw new ArgumentException($"Invalid parameter: {actualParameter}");
                                            }
                                            GenerateVariableStoringCode(formalParameter.AssociatedVariable, "t1");
                                        }
                                        nextCallParameters = [];
                                    }
                                    output?.Add($"jal ra, {callee.FullName}");
                                }
                                break;
                            case ReturnInstruction returnInstruction:
                                switch ((function.ReturnVariable == null, returnInstruction.Value == null))
                                {
                                    case (false, false):
                                        GenerateValueLoadingCode(returnInstruction.Value!, "t1");
                                        GenerateVariableStoringCode(function.ReturnVariable!, "t1");
                                        break;
                                    case (true, true):
                                        break;
                                    case (false, true):
                                        throw new CodeGeneratorException(returnInstruction.Position, $"Functions must return a value");
                                    case (true, false):
                                        throw new CodeGeneratorException(returnInstruction.Position, $"Procedures cannot return a value");
                                }
                                break;
                            case UnconditionalJumpInstruction unconditionalJumpInstruction:
                                output?.Add($"j l{unconditionalJumpInstruction.Target}");
                                break;
                            case ComparisonJumpInstruction comparisonJumpInstruction:
                                GenerateValueLoadingCode(comparisonJumpInstruction.Operand1, "t1");
                                GenerateValueLoadingCode(comparisonJumpInstruction.Operand1, "t2");
                                output?.Add($"{comparisonMap[comparisonJumpInstruction.Operation]} t1, t2, l{comparisonJumpInstruction.Target}");
                                break;
                            default:
                                throw new ArgumentException($"Invalid Intermediate Language Instruction: {instruction}");
                        }
                        currentIndex++;
                    }

                    void GenerateValueLoadingCode(Value value, string register)
                    {
                        if (value.IsConstant(out var constant))
                            output?.Add($"li {register}, {constant}");
                        else if (value.IsVariable(out var variable))
                            GenerateVariableLoadingCode(variable, register);
                    }

                    void GenerateVariableLoadingCode(Variable variable, string register)
                    {
                        GenerateVariableFetchingCode(variable);
                        output?.Add($"lw {register}, 0(t0)");
                    }

                    void GenerateVariableStoringCode(Variable variable, string register)
                    {
                        GenerateVariableFetchingCode(variable);
                        output?.Add($"sw {register}, 0(t0)");
                    }

                    /// <summary>
                    /// Generates a block of code that locates the specified variable in the stack.
                    /// After the generated code is executed, t0 holds the location of the variable.
                    /// The code uses only the register t0.
                    /// </summary>
                    /// <param name="variable">The variable to locate.</param>
                    /// <param name="output">The collection to store the generated code.</param>
                    void GenerateVariableFetchingCode(Variable variable)
                    {
                        // Dummy
                        output?.Add($"lw t0, 0(sp)");
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
