using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

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
        public static Instruction Create(Argument arg0, Argument arg1, Argument arg2, Argument arg3, int line, int column)
        {
            if (arg0.Value is Opcode opcode)
            {
                switch (opcode)
                {
                    case Opcode.Assignment:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not VariableSymbol variable) throw new SyntaxAnalyserException($"Expected Variable ID for 3rd argument: Line {line}, Column {column}");
                            return new AssignmentInstruction(line, column, variable, value);
                        }
                    case Opcode.Input:
                        {
                            if (arg1.Value is not VariableSymbol variable) throw new SyntaxAnalyserException($"Expected Variable ID for 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                            return new InputInstruction(line, column, variable);
                        }
                    case Opcode.Output:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                            return new OutputInstruction(line, column, value);
                        }
                    case Opcode.Halt:
                        {
                            if (arg1.Value is not null) throw new SyntaxAnalyserException($"Expected no 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                            return new HaltInstruction(line, column);
                        }
                    case Opcode.Jump:
                        {
                            if (arg1.Value is not null) throw new SyntaxAnalyserException($"Expected no 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not LabelSymbol label) throw new SyntaxAnalyserException($"Expected Label Name for 3rd argument: Line {line}, Column {column}");
                            UnconditionalJumpInstruction jump = new(line, column, null);
                            label.AddJump(jump);
                            return jump;
                        }
                    case Opcode.Parameter:
                        {
                            if (arg3.Value is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                            if (arg2.Value is ParameterType type) 
                            {
                                switch (type)
                                {
                                    case ParameterType.In:
                                        {
                                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 1st argument: Line {line}, Column {column}");
                                            return new ParameterInstruction(line, column, new InParameter(value));
                                        }
                                    case ParameterType.InOut:
                                        {
                                            if (arg1.Value is not VariableSymbol variable) throw new SyntaxAnalyserException($"Expected Variable ID for 1st argument: Line {line}, Column {column}");
                                            return new ParameterInstruction(line, column, new InOutParameter(variable));
                                        }
                                    case ParameterType.Out:
                                        {
                                            if (arg1.Value is not VariableSymbol variable) throw new SyntaxAnalyserException($"Expected Variable ID for 1st argument: Line {line}, Column {column}");
                                            return new ParameterInstruction(line, column, new OutParameter(variable));
                                        }
                                    default:
                                        break;
                                }
                            }
                            throw new SyntaxAnalyserException($"Expected Parameter Type for 2nd argument: Line {line}, Column {column}");
                        }
                    case Opcode.Call:
                        {
                            if (arg1.Value is not FunctionSymbol function) throw new SyntaxAnalyserException($"Expected Function Name for 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                            return new CallInstruction(line, column, function);
                        }
                    case Opcode.Return:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                            return new ReturnInstruction(line, column, value);
                        }
                    default:
                        throw new SyntaxAnalyserException($"{opcode} is not a valid Intermediate Language Instruction Opcode: Line {line}, Column {column}");
                }
            }
            else if (arg0.Value is OperatorToken.OperationType operation)
            {
                switch (OperatorToken.CategoryMap[operation])
                {
                    case OperatorToken.OperationCategory.Logical:
                    case OperatorToken.OperationCategory.Numerical:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value1)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is null || !Value.TryCast(arg2.Value, out Value? value2)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not VariableSymbol variable) throw new SyntaxAnalyserException($"Expected Variable ID for 3rd argument: Line {line}, Column {column}");
                            return new OperationInstruction(line, column, operation, value1, value2, variable);
                        }
                    case OperatorToken.OperationCategory.Comparison:
                        {
                            if (arg1.Value is null || !Value.TryCast(arg1.Value, out Value? value1)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 1st argument: Line {line}, Column {column}");
                            if (arg2.Value is null || !Value.TryCast(arg2.Value, out Value? value2)) throw new SyntaxAnalyserException($"Expected Constant or Variable ID for 2nd argument: Line {line}, Column {column}");
                            if (arg3.Value is not LabelSymbol label) throw new SyntaxAnalyserException($"Expected Label Name for 3rd argument: Line {line}, Column {column}");
                            ComparisonJumpInstruction jump = new(line, column, operation, value1, value2, null);
                            label.AddJump(jump);
                            return jump;
                        }
                    default:
                        throw new SyntaxAnalyserException($"{operation} is not a valid Operation: Line {line}, Column {column}");
                }
            }
            else throw new InvalidOperationException($"Expected Operation or Opcode for 0th argument");
        }
    }
}
