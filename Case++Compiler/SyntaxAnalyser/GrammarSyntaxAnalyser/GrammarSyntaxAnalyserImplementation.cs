using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

using System.Collections.Concurrent;

namespace CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser
{
    internal class GrammarSyntaxAnalyserImplementation : ISyntaxAnalyser
    {
        private readonly static TokenMatcher superMatcher;

        static GrammarSyntaxAnalyserImplementation()
        {
            TokenMatcher declarationsMatcher =
                "Variable Declarations" * 
                [
                    "Variable Declaration" ^
                    [
                        "\"declare\" Keyword" % typeof(DeclareToken),
                        "Variable List" ^
                        [
                            "Variable ID" % typeof(IdentifierToken) | (program => {
                                program.AddSymbol(new Variable(program.PopCompilerVariable<string>(), false));
                            }),
                            "More Variables" *
                            [
                                "Comma" % typeof(CommaToken),
                                "Variable ID" % typeof(IdentifierToken) | (program => {
                                    program.AddSymbol(new Variable(program.PopCompilerVariable<string>(), false));
                                }),
                            ],
                        ],
                    ],
                    "Semi Colon" % typeof(SemiColonToken),
                ];

            TokenMatcher formalParameterMatcher =
                "Formal Parameter" |
                [
                    "In Parameter" %
                    [
                        "Optional Keyword" ^
                            "\"in\" Keyword" % typeof(InToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (program => {
                        string id = program.PopCompilerVariable<string>();
                        Variable variable = new(id, false);
                        program.PeekCompilerVariable<Function>().AddParameter(new TypeRestrictedFormalParameter<InParameter>(variable));
                        program.InitialiseVariable(variable);
                    }),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (program => {
                        string id = program.PopCompilerVariable<string>();
                        Variable variable = new(id, true);
                        program.PeekCompilerVariable<Function>().AddParameter(new TypeRestrictedFormalParameter<OutParameter>(variable));
                    }),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (program => {
                        string id = program.PopCompilerVariable<string>();
                        Variable variable = new(id, true);
                        program.PeekCompilerVariable<Function>().AddParameter(new TypeRestrictedFormalParameter<InOutParameter>(variable));
                        program.InitialiseVariable(variable);
                    }),
                ];

            UnresolvedTokenMatcher statementMatcher = new("Statement");

            TokenMatcher functionsMatcher =
                "Functions" *
                (
                    "Function" %
                    [
                        "\"function\" Keyword" % typeof(FunctionToken),
                        "Function ID" % typeof(IdentifierToken) | (program => program.CreateFunction(program.PopCompilerVariable<string>())),
                        "Formal Parameter List" > (
                            "Formal Parameters" ^
                            [
                                formalParameterMatcher,
                                "More Parameters" *
                                [
                                    "Comma" % typeof(CommaToken),
                                    formalParameterMatcher,
                                ],
                            ]) | (program => _ = program.PopCompilerVariable<Function>() /* End the Formal Parameter definition */),
                        "Function Body" % statementMatcher,
                        "Optional Semi Colon" ^ typeof(SemiColonToken),
                    ] | (program => program.FinalizeFunction(p => new ReturnInstruction(p, 0)))
                );

            UnresolvedTokenMatcher expressionMatcher = new("Expression");

            TokenMatcher actualParameterMatcher =
                "Actual Parameter" |
                [
                    "In Parameter" %
                    [
                        "Optional Keyword" ^
                            "\"in\" Keyword" % typeof(InToken),
                        expressionMatcher,
                    ] | (program => {
                        Value value = program.PopCompilerVariable<ExpressionBlockInfo>().Result;
                        program.PeekCompilerVariable<FunctionCallBlockInfo>().AddParameter(new InParameter(value));
                    }),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (program => {
                        Variable variable = program.GetSymbolInScope<Variable>(program.PopCompilerVariable<string>());
                        program.PeekCompilerVariable<FunctionCallBlockInfo>().AddParameter(new OutParameter(variable));
                    }),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (program => {
                        Variable variable = program.GetSymbolInScope<Variable>(program.PopCompilerVariable<string>());
                        program.UseVariable(variable);
                        program.PeekCompilerVariable < FunctionCallBlockInfo >().AddParameter(new InOutParameter(variable));
                    }),
                ];

            TokenMatcher factorMatcher =
                "Factor" |
                [
                    "Constant" % typeof(ConstantToken) | (program => program.PushCompilerVariable(new ExpressionBlockInfo(program.PopCompilerVariable<uint>(), program.CurrentInstructionIndex))),
                    "Sub-Expression" > expressionMatcher,
                    "Identifier" %
                    [
                        "Name" % typeof(IdentifierToken),
                        "Optional Parameters" |
                        [
                            "Actual Parameter List" >
                            [
                                -"$function" | (program => {
                                    program.PushCompilerVariable(new FunctionCallBlockInfo(program.GetSymbolInScope<Function>(program.PopCompilerVariable<string>()), program.CurrentInstructionIndex));
                                }),
                                "Actual Parameters" ^
                                [
                                    actualParameterMatcher,
                                    "More Parameters" *
                                    [
                                        "Comma" % typeof(CommaToken),
                                        actualParameterMatcher,
                                    ],
                                ],
                                -"$call" | (program => {
                                    FunctionCallBlockInfo callBlockInfo = program.PopCompilerVariable<FunctionCallBlockInfo>();
                                    Variable result = program.GenerateTemp();
                                    callBlockInfo.AddParameter(new OutParameter(result));
                                    foreach (var parameter in callBlockInfo.Parameters)
                                    {
                                        program.CreateInstruction(p => new ParameterInstruction(p, parameter));
                                        if (parameter is OutParameter par) program.InitialiseVariable(par.Variable);
                                    }
                                    program.CreateInstruction(p => new CallInstruction(p, callBlockInfo.Function));
                                    program.PushCompilerVariable(new ExpressionBlockInfo(result, callBlockInfo.Start));
                                    program.MergeVariableDependancies(callBlockInfo.Function);
                                }),
                            ],
                            -"$variable" | (program => {
                                // If there are no parameters, expect an initialised variable
                                Variable variable = program.GetSymbolInScope<Variable>(program.PopCompilerVariable<string>());
                                program.UseVariable(variable);
                                program.PushCompilerVariable(new ExpressionBlockInfo(variable, program.CurrentInstructionIndex));
                            }),
                        ],
                    ],
                ];

            TokenMatcher termMatcher =
                "Term" %
                [
                    factorMatcher,
                    "More Factors" * (
                        "Operation" %
                        [
                            "Multiplicative Operation" |
                            [
                                "Multiply" % OperationType.Multiply,
                                "Divide" % OperationType.Divide,
                            ],
                            factorMatcher,
                        ] | (program => {
                            ExpressionBlockInfo operand2 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            OperationType operation = program.PopCompilerVariable<OperationType>();
                            ExpressionBlockInfo operand1 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            Variable temp = program.GenerateTemp();
                            program.CreateInstruction(p => new OperationInstruction(p, operation, operand1.Result, operand2.Result, temp));
                            program.PushCompilerVariable(new ExpressionBlockInfo(temp, operand1.Start));
                        })),
                ];

            expressionMatcher.Resolve(
                "Expression" %
                [
                    "First Term" |
                    [
                        "Signed Term" %
                        [
                            "Sign" |
                            [
                                "Plus" % OperationType.Add,
                                "Minus" % OperationType.Subtract,
                            ],
                            termMatcher,
                            -"$apply sign" | (program => {
                                ExpressionBlockInfo term = program.PopCompilerVariable<ExpressionBlockInfo>();
                                OperationType sign = program.PopCompilerVariable<OperationType>();
                                Variable temp = program.GenerateTemp();
                                program.CreateInstruction(p => new OperationInstruction(p, sign, 0, term.Result, temp));
                                program.PushCompilerVariable(new ExpressionBlockInfo(temp, term.Start));
                            }),
                        ],
                        "Unsigned Term" % termMatcher,
                    ],
                    "More Terms" * (
                        "Operation" %
                        [
                            "Additive Operation" |
                            [
                                "Add" % OperationType.Add,
                                "Subtract" % OperationType.Subtract,
                            ],
                            termMatcher,
                        ] | (program => {
                            ExpressionBlockInfo operand2 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            OperationType operation = program.PopCompilerVariable<OperationType>();
                            ExpressionBlockInfo operand1 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            Variable temp = program.GenerateTemp();
                            program.CreateInstruction(p => new OperationInstruction(p, operation, operand1.Result, operand2.Result, temp));
                            program.PushCompilerVariable(new ExpressionBlockInfo(temp, operand1.Start));
                        })),
                ]);

            UnresolvedTokenMatcher conditionMatcher = new("Condition");

            TokenMatcher boolFactorMatcher =
                "Bool Factor" |
                [
                    "Constant" % typeof(BoolConstantToken) | (program =>
                    {
                        int start = program.CurrentInstructionIndex;
                        JumpInstruction jump = program.CreateInstruction(p => new UnconditionalJumpInstruction(p));

                        bool @bool = program.PopCompilerVariable<bool>();
                        List<JumpInstruction> trueOriginList  = [];
                        List<JumpInstruction> falseOriginList = [];
                        if (@bool == true) trueOriginList.Add(jump);
                        else              falseOriginList.Add(jump);
                        program.PushCompilerVariable(new JumpBlockInfo(trueOriginList, falseOriginList, start));
                    }),
                    "Inverted Sub-Condition" %
                    [
                        "\"not\" Keyword" % OperationType.Not,
                        "Sub-Condition" >= conditionMatcher,
                    ] | (program => {
                        JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                        _ = program.PopCompilerVariable<OperationType>(); // Ignore Variable; it will always be Not.
                        program.PushCompilerVariable(new JumpBlockInfo(info.FalseOriginList, info.TrueOriginList, info.Start));
                    }),
                    "Sub-Condition" >= conditionMatcher,
                    "Comparison" %
                    [
                        expressionMatcher,
                        "Comparison Operation" |
                        [
                            "Equality" % OperationType.EqualTo,
                            "Less Than" % OperationType.LessThan,
                            "Greater Than" % OperationType.GreaterThan,
                            "Inequality" % OperationType.NotEqualTo,
                            "Less Than or Equal To" % OperationType.LessThanOrEqualTo,
                            "Greater Than or Equal To" % OperationType.GreaterThanOrEqualTo,
                        ],
                        expressionMatcher,
                    ] | (program => {
                        ExpressionBlockInfo operand2 = program.PopCompilerVariable<ExpressionBlockInfo>();
                        OperationType operation = program.PopCompilerVariable<OperationType>();
                        ExpressionBlockInfo operand1 = program.PopCompilerVariable<ExpressionBlockInfo>();
                        program.AddJumpInstructions(
                            p => new ComparisonJumpInstruction(p, operation, operand1.Result, operand2.Result),
                            operand1.Start);
                    }),
                ];

            TokenMatcher boolTermMatcher =
                "Bool Term" %
                [
                    boolFactorMatcher,
                    "More Bool Factors" * (
                        "Operation" %
                        [
                            "And" % OperationType.And,
                            boolFactorMatcher,
                        ] | (program => {
                            JumpBlockInfo info2 = program.PopCompilerVariable<JumpBlockInfo>();
                            _ = program.PopCompilerVariable<OperationType>(); // Ignore Variable; it will always be And.
                            JumpBlockInfo info1 = program.PopCompilerVariable<JumpBlockInfo>();

                            info1.TrueOriginList.Targets = info2.Start;
                            info1.FalseOriginList.AddRange(info2.FalseOriginList);
                            program.PushCompilerVariable(new JumpBlockInfo(info2.TrueOriginList, info1.FalseOriginList, info1.Start));
                        })),
                ];

            conditionMatcher.Resolve(
                "Condition" %
                [
                    boolTermMatcher,
                    "More Bool Terms" * (
                        "Operation" %
                        [
                            "Or" % OperationType.Or,
                            boolTermMatcher,
                        ] | (program => {
                            JumpBlockInfo info2 = program.PopCompilerVariable<JumpBlockInfo>();
                            _ = program.PopCompilerVariable<OperationType>(); // Ignore Variable; it will always be Or.
                            JumpBlockInfo info1 = program.PopCompilerVariable<JumpBlockInfo>();

                            info1.FalseOriginList.Targets = info2.Start;
                            info1.TrueOriginList.AddRange(info2.TrueOriginList);
                            program.PushCompilerVariable(new JumpBlockInfo(info1.TrueOriginList, info2.FalseOriginList, info1.Start));
                        })),
                ]);

            UnresolvedTokenMatcher ILInstruction = new("IL Instruction");

            TokenMatcher ILArgumentMatcher =
                "Argument" |
                [
                    "Constant" % typeof(ConstantToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(program.PopCompilerVariable<uint>()))),
                    "Symbol or Label" % typeof(IdentifierToken) | (program => {
                        string name = program.PopCompilerVariable<string>();
                        if (!program.TryGetSymbolInScope<Symbol>(name, out var symbol))
                        {
                            symbol = new Label(name, []);
                            program.AddSymbol(symbol);
                        }
                        program.PushCompilerVariable(new InstructionFactory.Argument(symbol));
                    }),
                    "Underscore" % typeof(UnderscoreToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(null))),
                    "CV" % typeof(CVToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.In))),
                    "Ref" % typeof(RefToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.InOut))),
                    "Ret" % typeof(RetToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.Out))),
                ];

            ILInstruction.Resolve(
                "Intermediate Language Instruction" %
                [
                    "Label" ^
                        "Label" %
                        [
                            "Label Name" % typeof(IdentifierToken),
                            "Colon" % typeof(ColonToken),
                        ] | (program => program.SetLabel(program.PopCompilerVariable<string>())),
                    "Instruction" ^
                    [
                        "Valid Opcode" |
                        [
                            "Assignment Token" % typeof(AssignmentToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Assignment))),
                            "In Token" % typeof(InToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Input))),
                            "Out Token" % typeof(OutToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Output))),
                            "Halt Token" % typeof(HaltToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Halt))),
                            "Jump Token" % typeof(JumpToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Jump))),
                            "Par Token" % typeof(ParToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Parameter))),
                            "Call Token" % typeof(CallToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Call))),
                            "Retv Token" % typeof(RetvToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Return))),
                            "Operation" % typeof(OperatorToken) | (program => program.PushCompilerVariable(new InstructionFactory.Argument(program.PopCompilerVariable<OperationType>()))),
                        ],
                        "Comma" % typeof(CommaToken),
                        ILArgumentMatcher,
                        "Comma" % typeof(CommaToken),
                        ILArgumentMatcher,
                        "Comma" % typeof(CommaToken),
                        ILArgumentMatcher,
                    ] | (program => {
                        InstructionFactory.Argument arg3 = program.PopCompilerVariable<InstructionFactory.Argument>();
                        InstructionFactory.Argument arg2 = program.PopCompilerVariable<InstructionFactory.Argument>();
                        InstructionFactory.Argument arg1 = program.PopCompilerVariable<InstructionFactory.Argument>();
                        InstructionFactory.Argument arg0 = program.PopCompilerVariable<InstructionFactory.Argument>();
                        program.AddIntermediateLanguageInstruction(arg0, arg1, arg2, arg3);
                    })
                ]);
            
            UnresolvedTokenMatcher blockBodyMatcher = new("Block Body");

            statementMatcher.Resolve(
                "Statement" |
                [
                    blockBodyMatcher,
                    "Assignment" %
                    [
                        "Variable ID" % typeof(IdentifierToken),
                        "Assignment Token" % typeof(AssignmentToken),
                        expressionMatcher,
                    ] | (program => {
                        ExpressionBlockInfo expression = program.PopCompilerVariable<ExpressionBlockInfo>();
                        Variable variable = program.GetSymbolInScope<Variable>(program.PopCompilerVariable<string>());
                        // Assume that the variable is initialised when the first assignment is encountered.
                        // It is possible that this assignment will be skipped, but it's hard to check initisation in a way that accounts for jumps.
                        // This assumption doesn't lead to false positives and catches the most obvious (but not uncommon) true positives.
                        program.CreateInstruction(p => new AssignmentInstruction(p, variable, expression.Result));
                        program.InitialiseVariable(variable);
                    }),
                    "If" %
                    [
                        "\"if\" Keyword" % typeof(IfToken),
                        conditionMatcher,
                        -"$true target" | (program => {
                            JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                        }),
                        "If Body" ^ statementMatcher,
                        "Optional Else" ^
                            "Else" %
                            [
                                // "Optional Semi Colon" ^ typeof(SemiColonToken),
                                // Cannot allow a semi colon here because it's ambiguous
                                // whether a semi colon ends the main body or the whole if
                                "\"else\" Keyword" % typeof(ElseToken),
                                -"$false target" | (program => {
                                    int ifExit = program.CurrentInstructionIndex;
                                    JumpInstruction exitJump = program.CreateInstruction(p => new UnconditionalJumpInstruction(p));

                                    JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                    info.FalseOriginList.Targets = program.CurrentInstructionIndex;

                                    program.PushCompilerVariable(new JumpBlockInfo([], [exitJump], ifExit));
                                }),
                                "Else Body" ^ statementMatcher,
                            ],
                        -"$end" | (program => {
                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                        }),
                    ],
                    "Switch Case" %
                    [
                        "\"switchcase\" Keyword" % typeof(SwitchCaseToken),
                        -"$start" | (program => {
                            List<JumpInstruction> exitJumps = [];
                            program.PushCompilerVariable(exitJumps);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (program => {
                                JumpInstruction exitJump = program.CreateInstruction(p => new UnconditionalJumpInstruction(p));

                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;

                                List<JumpInstruction> exitJumps = program.PeekCompilerVariable<List<JumpInstruction>>();
                                exitJumps.Add(exitJump);
                            }),
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(ColonToken),
                        "Default Body" ^ statementMatcher,
                        -"$end" | (program => {
                            List<JumpInstruction> exitJumps = program.PopCompilerVariable<List<JumpInstruction>>();
                            exitJumps.Targets = program.CurrentInstructionIndex;
                        }),
                    ],
                    "While" %
                    [
                        "\"while\" Keyword" % typeof(WhileToken) | (program => program.SetRepeatPoint()),
                        conditionMatcher,
                        -"$true target" | (program => {
                            JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                        }),
                        "While Body" ^ statementMatcher,
                        -"$repeat" | (program => {
                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            program.CreateInstruction(p => new UnconditionalJumpInstruction(p) { Target = info.Start });
                            info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                        }),
                        "Optional Else" ^
                            "Else" %
                            [
                                // "Optional Semi Colon" ^ typeof(SemiColonToken),
                                // Cannot allow a semi colon here because it's ambiguous
                                // whether a semi colon ends the main body or the whole while
                                "\"else\" Keyword" % typeof(ElseToken),
                                "Else Body" ^ statementMatcher,
                            ],
                        -"$end" | (program => program.SetBreakPoint()),
                    ],
                    "While Case" %
                    [
                        "\"whilecase\" Keyword" % typeof(WhileCaseToken) | (program => program.SetRepeatPoint()),
                        -"$start" | (program => {
                            int start = program.CurrentInstructionIndex;
                            program.PushCompilerVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$repeat & false target" | (program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                int start = program.PeekCompilerVariable<int>();
                                program.CreateInstruction(p => new UnconditionalJumpInstruction(p) { Target = start });
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        -"$remove start variable" | (program => _ = program.PopCompilerVariable<int>()),
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(ColonToken),
                        "Default Body" ^ statementMatcher,
                        -"$end" | (program => program.SetBreakPoint()),
                    ],
                    "Until Case" %
                    [
                        "\"untilcase\" Keyword" % typeof(UntilCaseToken) | (program => program.SetRepeatPoint()),
                        -"$start" | (program => {
                            int start = program.CurrentInstructionIndex;
                            program.PushCompilerVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        "\"until\" Keyword" % typeof(UntilToken),
                        conditionMatcher,
                        -"$until" | (program => {
                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = program.CurrentInstructionIndex;

                            int start = program.PopCompilerVariable<int>();
                            info.FalseOriginList.Targets = start;

                            program.SetBreakPoint();
                        }),
                    ],
                    "In Case" %
                    [
                        "\"incase\" Keyword" % typeof(InCaseToken) | (program => program.SetRepeatPoint()),
                        -"$initialize" | (program => {
                            int start = program.CurrentInstructionIndex;
                            program.PushCompilerVariable(start);

                            Variable exitFlag = program.GenerateTemp();
                            program.PushCompilerVariable(exitFlag);
                            program.CreateInstruction(p => new AssignmentInstruction(p, exitFlag, 0));
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole incase
                            -"$false target" | (program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                Variable exitFlag = program.PeekCompilerVariable<Variable>();

                                program.CreateInstruction(p => new AssignmentInstruction(p, exitFlag, 1));
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        -"$end" | (program =>
                        {
                            Variable exitFlag = program.PopCompilerVariable<Variable>();
                            int start = program.PopCompilerVariable<int>();
                            program.CreateInstruction(p => new ComparisonJumpInstruction(p, OperationType.NotEqualTo, exitFlag, 0) { Target = start });

                            program.SetBreakPoint();
                        }),
                    ],
                    "For Case" %
                    [
                        "\"forcase\" Keyword" % typeof(ForCaseToken) | (program => program.SetRepeatPoint()),
                        "Iteration Identifier" % typeof(IdentifierToken),
                        -"$initialize" | (program => {
                            Variable iterationVariable = new(program.PopCompilerVariable<string>(), false);
                            program.AddSymbol(iterationVariable);
                            program.InitialiseVariable(iterationVariable);
                            program.PushCompilerVariable(iterationVariable);
                            program.CreateInstruction(p => new AssignmentInstruction(p, iterationVariable, 0));
                        }),
                        "Equals Sign" % OperationType.EqualTo | (program => {
                            _ = program.PopCompilerVariable<OperationType>(); // Ignore Variable; it will always be an Equals Sign.
                        }),
                        expressionMatcher,
                        -"$condition & increment" | (program =>
                        {
                            // Condition

                            ExpressionBlockInfo count = program.PopCompilerVariable<ExpressionBlockInfo>();
                            Variable iterationVariable = program.PopCompilerVariable<Variable>();
                            program.AddJumpInstructions(
                                p => new ComparisonJumpInstruction(p, OperationType.LessThan, iterationVariable, count.Result),
                                program.CurrentInstructionIndex);

                            JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = program.CurrentInstructionIndex;

                            // Increment

                            program.CreateInstruction(p => new OperationInstruction(p, OperationType.Add, iterationVariable, 1, iterationVariable));

                            program.SetRepeatPoint();
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole forcase
                            -"$false target" | (program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        -"$end" | (program => {
                            program.SetBreakPoint();

                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            program.CreateInstruction(p => new UnconditionalJumpInstruction(p) { Target = info.Start });
                            info.FalseOriginList.Targets = program.CurrentInstructionIndex;

                            program.SetBreakPoint();
                        }),
                    ],
                    "Break" %
                    [
                        "\"break\" Keyword" % typeof(BreakToken),
                        "Break Count" % typeof(ConstantToken),
                    ] | (program => program.AddBreakInstruction(program.PopCompilerVariable<uint>())),
                    "Repeat" %
                    [
                        "\"repeat\" Keyword" % typeof(RepeatToken),
                        "Repeat Index" % typeof(ConstantToken),
                    ] | (program => program.AddRepeatInstruction(program.PopCompilerVariable<uint>())),
                    "Return" %
                    [
                        "\"return\" Keyword" % typeof(ReturnToken),
                        expressionMatcher,
                    ] | (program => program.CreateInstruction(p => new ReturnInstruction(p, program.PopCompilerVariable < ExpressionBlockInfo >().Result))),
                    "Input" %
                    [
                        "\"input\" Keyword" % typeof(InputToken),
                        "Variable ID" % typeof(IdentifierToken),
                    ] | (program => {
                        Variable variable = program.GetSymbolInScope<Variable>(program.PopCompilerVariable<string>());
                        program.CreateInstruction(p => new InputInstruction(p, variable));
                        program.InitialiseVariable(variable);
                    }),
                    "Print" %
                    [
                        "\"print\" Keyword" % typeof(PrintToken),
                        expressionMatcher,
                    ] | (program => program.CreateInstruction(p => new OutputInstruction(p, program.PopCompilerVariable < ExpressionBlockInfo >().Result))),
                    "Intermediate Language Instruction" %
                    [
                        "hash prefix" % typeof(HashToken),
                        "Instruction or Block" |
                        [
                            ILInstruction,
                            "Intermediate Language Block" >>
                                "Instructions" * ILInstruction,
                        ],
                    ],
                ]);

            TokenMatcher statementsMatcher =
                "Statements" %
                [
                    "Statement" ^ statementMatcher,
                    "Continuation" *
                    [
                        "Semi Colon" % typeof(SemiColonToken),
                        "Statement" ^ statementMatcher,
                    ],
                ];

            blockBodyMatcher.Resolve(
                "Body" >>
                    "Body Contents" %
                    [
                        -"$start" | (program => {
                            program.EnterScope();
                            program.SetRepeatPoint();
                        }),
                        declarationsMatcher,
                        functionsMatcher,
                        statementsMatcher,
                        -"$end" | (program => {
                            program.ExitScope();
                            program.SetBreakPoint();
                        }),
                    ]);

            superMatcher =
                "Program" %
                [
                    "\"program\" Keyword" % typeof(ProgramToken),
                    "Program ID" % typeof(IdentifierToken) | (program => {
                        program.CreateFunction(program.PopCompilerVariable<string>(), true);
                        _ = program.PopCompilerVariable<Function>(); // Ignore variable; program takes no arguments.
                    }),
                    "Program Body" % statementMatcher,
                    "EOF" % typeof(EOFToken) | (program => program.FinalizeFunction(p => new HaltInstruction(p))),
                ];
        }

        public void Analyse(BlockingCollection<Token> input, IntermediateProgram? output = null)
        {
            var tokens = input.GetConsumingEnumerable().GetEnumerator();
            if (!tokens.MoveNext()) throw new SyntaxAnalyserException(default, $"Expected EOF Token.");
            if (superMatcher.TryMatch(tokens, output) == false)
                throw new SyntaxAnalyserException(tokens.Current.Position, $"Expected {superMatcher.Name}, but got {tokens.Current}.");
            output?.CheckNoLeftoverVariables();
        }
    }
}