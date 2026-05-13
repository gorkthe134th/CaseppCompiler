namespace CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions
{
    internal static class InstructionFactory
    {
        public enum Opcode
        {
            Assignment,
            Input,
            Output,
            Halt,
            Jump,
            Parameter,
            Call,
            Return,
        }

        public enum ParameterType
        {
            In,
            InOut,
            Out,
        }

        public record Argument(object? Value) { }

        // TODO: Add initialisation checking
        public static Instruction Create(Argument arg0, Argument arg1, Argument arg2, Argument arg3, Position position, CancellationToken? cancellationToken = null)
        {
            if (arg0.Value is Opcode opcode)
            {
                switch (opcode)
                {
                    case Opcode.Assignment:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 1st argument.");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 2nd argument.");
                            if (arg3.Value is not Variable variable) throw new SyntaxAnalyserException(position, $"Expected Variable ID for 3rd argument.");
                            return new AssignmentInstruction(position, variable, value);
                        }
                    case Opcode.Input:
                        {
                            if (arg1.Value is not Variable variable) throw new SyntaxAnalyserException(position, $"Expected Variable ID for 1st argument.");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 2nd argument.");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 3rd argument.");
                            return new InputInstruction(position, variable);
                        }
                    case Opcode.Output:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 1st argument.");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 2nd argument.");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 3rd argument.");
                            return new OutputInstruction(position, value);
                        }
                    case Opcode.Halt:
                        {
                            if (arg1.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 1st argument.");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 2nd argument.");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 3rd argument.");
                            return new HaltInstruction(position);
                        }
                    case Opcode.Jump:
                        {
                            if (arg1.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 1st argument.");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 2nd argument.");
                            if (arg3.Value is not Label label) throw new SyntaxAnalyserException(position, $"Expected Label Name for 3rd argument.");
                            UnconditionalJumpInstruction jump = new(position, cancellationToken);
                            label.AddJump(jump);
                            return jump;
                        }
                    case Opcode.Parameter:
                        {
                            if (arg3.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 3rd argument.");
                            if (arg2.Value is ParameterType type) 
                            {
                                switch (type)
                                {
                                    case ParameterType.In:
                                        {
                                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 1st argument.");
                                            return new ParameterInstruction(position, new InParameter(value));
                                        }
                                    case ParameterType.InOut:
                                        {
                                            if (arg1.Value is not Variable variable) throw new SyntaxAnalyserException(position, $"Expected Variable ID for 1st argument.");
                                            return new ParameterInstruction(position, new InOutParameter(variable));
                                        }
                                    case ParameterType.Out:
                                        {
                                            if (arg1.Value is not Variable variable) throw new SyntaxAnalyserException(position, $"Expected Variable ID for 1st argument.");
                                            return new ParameterInstruction(position, new OutParameter(variable));
                                        }
                                    default:
                                        break;
                                }
                            }
                            throw new SyntaxAnalyserException(position, $"Expected Parameter Type for 2nd argument.");
                        }
                    case Opcode.Call:
                        {
                            if (arg1.Value is not Function function) throw new SyntaxAnalyserException(position, $"Expected Function Name for 1st argument.");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 2nd argument.");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 3rd argument.");
                            return new CallInstruction(position, function);
                        }
                    case Opcode.Return:
                        {
                            Value? value = null;
                            if (arg1.Value is not null && !Value.TryCast(arg1.Value, out value)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 1st argument.");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 2nd argument.");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException(position, $"Expected no 3rd argument.");
                            return new ReturnInstruction(position, value);
                        }
                    default:
                        throw new SyntaxAnalyserException(position, $"{opcode} is not a valid Intermediate Language Instruction Opcode.");
                }
            }
            else if (arg0.Value is OperationType operation)
            {
                switch (operation.Category)
                {
                    case OperationCategory.Logical:
                    case OperationCategory.Numerical:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value1)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 1st argument.");
                            if (arg2.Value is null || !Value.TryCast(arg2.Value, out Value? value2)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 2nd argument.");
                            if (arg3.Value is not Variable variable) throw new SyntaxAnalyserException(position, $"Expected Variable ID for 3rd argument.");
                            return new OperationInstruction(position, operation, value1, value2, variable);
                        }
                    case OperationCategory.Comparison:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value1)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 1st argument.");
                            if (arg2.Value is null || !Value.TryCast(arg2.Value, out Value? value2)) throw new SyntaxAnalyserException(position, $"Expected Constant or Variable ID for 2nd argument.");
                            if (arg3.Value is not Label label) throw new SyntaxAnalyserException(position, $"Expected Label Name for 3rd argument.");
                            ComparisonJumpInstruction jump = new(position, operation, value1, value2, cancellationToken);
                            label.AddJump(jump);
                            return jump;
                        }
                    default:
                        throw new SyntaxAnalyserException(position, $"{operation} is not a valid Operation.");
                }
            }
            else throw new InvalidOperationException($"Expected Operation or Opcode for 0th argument.");
        }
    }
}
