using CaseppCompiler.LexicalAnalyser.Tokens;

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

        public static Instruction Create(Opcode opcode, object arg1, object arg2, object arg3, int line, int column)
        {
            switch (opcode)
            {
                case Opcode.Assignment:
                    {
                        if (arg1 is null) throw new SyntaxAnalyserException($"Expected 1st argument: Line {line}, Column {column}");
                        if (arg2 is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                        if (arg3 is not string varID) throw new SyntaxAnalyserException($"Expected Variable ID for 3rd argument: Line {line}, Column {column}");
                        return new AssignmentInstruction(line, column, varID, arg1);
                    }
                case Opcode.Input:
                    {
                        if (arg1 is not string varID) throw new SyntaxAnalyserException($"Expected Variable ID for 1st argument: Line {line}, Column {column}");
                        if (arg2 is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                        if (arg3 is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                        return new InputInstruction(line, column, varID);
                    }
                case Opcode.Output:
                    {
                        if (arg1 is null) throw new SyntaxAnalyserException($"Expected 1st argument: Line {line}, Column {column}");
                        if (arg2 is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                        if (arg3 is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                        return new OutputInstruction(line, column, arg1);
                    }
                case Opcode.Halt:
                    {
                        if (arg1 is not null) throw new SyntaxAnalyserException($"Expected no 1st argument: Line {line}, Column {column}");
                        if (arg2 is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                        if (arg3 is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                        return new HaltInstruction(line, column);
                    }
                case Opcode.Jump:
                    {
                        if (arg1 is not null) throw new SyntaxAnalyserException($"Expected no 1st argument: Line {line}, Column {column}");
                        if (arg2 is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                        return new UnconditionalJumpInstruction(line, column, null);
                    }
                case Opcode.Parameter:
                    {
                        if (arg1 is null) throw new SyntaxAnalyserException($"Expected 1st argument: Line {line}, Column {column}");
                        if (arg2 is not ParameterInstruction.ParameterType type) throw new SyntaxAnalyserException($"Expected Parameter Type for 2nd argument: Line {line}, Column {column}");
                        if (arg3 is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                        return new ParameterInstruction(line, column, arg1, type);
                    }
                case Opcode.Call:
                    {
                        if (arg1 is not string function) throw new SyntaxAnalyserException($"Expected Function Name for 1st argument: Line {line}, Column {column}");
                        if (arg2 is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                        if (arg3 is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                        return new CallInstruction(line, column, function);
                    }
                case Opcode.Return:
                    {
                        if (arg1 is null) throw new SyntaxAnalyserException($"Expected 1st argument: Line {line}, Column {column}");
                        if (arg2 is not null) throw new SyntaxAnalyserException($"Expected no 2nd argument: Line {line}, Column {column}");
                        if (arg3 is not null) throw new SyntaxAnalyserException($"Expected no 3rd argument: Line {line}, Column {column}");
                        return new ReturnInstruction(line, column, arg1);
                    }
                default:
                    throw new SyntaxAnalyserException($"{opcode} is not a valid Intermediate Language Instruction Opcode: Line {line}, Column {column}");
            };
        }

        public static Instruction Create(OperatorToken.OperationType operation, object arg1, object arg2, object arg3, int line, int column)
        {
            switch (OperatorToken.CategoryMap[operation])
            {
                case OperatorToken.OperationCategory.Logical:
                case OperatorToken.OperationCategory.Numerical:
                    if (arg1 is null) throw new SyntaxAnalyserException($"Expected 1st argument: Line {line}, Column {column}");
                    if (arg2 is null) throw new SyntaxAnalyserException($"Expected 2nd argument: Line {line}, Column {column}");
                    if (arg3 is not string varID) throw new SyntaxAnalyserException($"Expected Variable ID for 3rd argument: Line {line}, Column {column}");
                    return new OperationInstruction(line, column, operation, arg1, arg2, varID);
                case OperatorToken.OperationCategory.Comparison:
                    if (arg1 is null) throw new SyntaxAnalyserException($"Expected 1st argument: Line {line}, Column {column}");
                    if (arg2 is null) throw new SyntaxAnalyserException($"Expected 2nd argument: Line {line}, Column {column}");
                    return new ComparisonJumpInstruction(line, column, operation, arg1, arg2, null);
                default:
                    throw new SyntaxAnalyserException($"{operation} is not a valid Operation Type: Line {line}, Column {column}");
            }
        }
    }
}
