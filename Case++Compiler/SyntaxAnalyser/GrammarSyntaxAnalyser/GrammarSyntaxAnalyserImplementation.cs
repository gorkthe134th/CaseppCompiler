using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;

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
                            "Variable ID" % typeof(IdentifierToken) | (async program => {
                                program.AddSymbol(new Variable(program.PopCompilerVariable<string>(), false, false));
                            }),
                            "More Variables" *
                            [
                                "Comma" % typeof(CommaToken),
                                "Variable ID" % typeof(IdentifierToken) | (async program => {
                                    program.AddSymbol(new Variable(program.PopCompilerVariable<string>(), false, false));
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
                    ] | (async program => {
                        string id = program.PopCompilerVariable<string>();
                        Variable variable = new(id, false, false);
                        program.AddFormalParameter(new TypeRestrictedFormalParameter<InParameter>(variable));
                        program.InitialiseVariable(variable);
                    }),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (async program => {
                        string id = program.PopCompilerVariable<string>();
                        Variable variable = new(id, true, true);
                        program.AddFormalParameter(new TypeRestrictedFormalParameter<OutParameter>(variable));
                    }),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (async program => {
                        string id = program.PopCompilerVariable<string>();
                        Variable variable = new(id, true, false);
                        program.AddFormalParameter(new TypeRestrictedFormalParameter<InOutParameter>(variable));
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
                            ]) | (async program => program.AddReturnValueParameter()),
                        "Function Body" % statementMatcher,
                        "Optional Semi Colon" ^ typeof(SemiColonToken),
                    ] | (program => program.FinalizeFunction((p, ct) => new ReturnInstruction(p, 0)))
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
                    ] | (async program => {
                        Value value = program.PopCompilerVariable<ExpressionBlockInfo>().Result;
                        program.PeekCompilerVariable<FunctionCallBlockInfo>().AddParameter(new InParameter(value));
                    }),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (async program => {
                        Variable variable = program.GetAccessibleSymbol<Variable>(program.PopCompilerVariable<string>());
                        program.PeekCompilerVariable<FunctionCallBlockInfo>().AddParameter(new OutParameter(variable));
                    }),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (async program => {
                        Variable variable = program.GetAccessibleSymbol<Variable>(program.PopCompilerVariable<string>());
                        program.UseVariable(variable);
                        program.PeekCompilerVariable < FunctionCallBlockInfo >().AddParameter(new InOutParameter(variable));
                    }),
                ];

            TokenMatcher factorMatcher =
                "Factor" |
                [
                    "Constant" % typeof(ConstantToken) | (async program => program.PushCompilerVariable(new ExpressionBlockInfo(program.PopCompilerVariable<uint>(), program.CurrentInstructionIndex))),
                    "Sub-Expression" > expressionMatcher,
                    "Identifier" %
                    [
                        "Name" % typeof(IdentifierToken),
                        "Optional Parameters" |
                        [
                            "Actual Parameter List" >
                            [
                                -"$function" | (async program => {
                                    program.PushCompilerVariable(new FunctionCallBlockInfo(program.GetAccessibleSymbol<Function>(program.PopCompilerVariable<string>()), program.CurrentInstructionIndex));
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
                                -"$call" | (async program => {
                                    FunctionCallBlockInfo callBlockInfo = program.PopCompilerVariable<FunctionCallBlockInfo>();
                                    Variable result = program.GenerateTemp();
                                    callBlockInfo.AddParameter(new OutParameter(result));
                                    foreach (var parameter in callBlockInfo.Parameters)
                                    {
                                        await program.CreateInstruction((p, ct) => new ParameterInstruction(p, parameter));
                                        if (parameter is OutParameter par) program.InitialiseVariable(par.Variable);
                                    }
                                    await program.CreateInstruction((p, ct) => new CallInstruction(p, callBlockInfo.Function));
                                    program.PushCompilerVariable(new ExpressionBlockInfo(result, callBlockInfo.Start));
                                    program.MergeVariableDependancies(callBlockInfo.Function);
                                }),
                            ],
                            -"$variable" | (async program => {
                                // If there are no parameters, expect an initialised variable
                                Variable variable = program.GetAccessibleSymbol<Variable>(program.PopCompilerVariable<string>());
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
                        ] | (async program => {
                            ExpressionBlockInfo operand2 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            OperationType operation = program.PopCompilerVariable<OperationType>();
                            ExpressionBlockInfo operand1 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            Variable temp = program.GenerateTemp();
                            await program.CreateInstruction((p, ct) => new OperationInstruction(p, operation, operand1.Result, operand2.Result, temp));
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
                            -"$apply sign" | (async program => {
                                ExpressionBlockInfo term = program.PopCompilerVariable<ExpressionBlockInfo>();
                                OperationType sign = program.PopCompilerVariable<OperationType>();
                                Variable temp = program.GenerateTemp();
                                await program.CreateInstruction((p, ct) => new OperationInstruction(p, sign, 0, term.Result, temp));
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
                        ] | (async program => {
                            ExpressionBlockInfo operand2 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            OperationType operation = program.PopCompilerVariable<OperationType>();
                            ExpressionBlockInfo operand1 = program.PopCompilerVariable<ExpressionBlockInfo>();
                            Variable temp = program.GenerateTemp();
                            await program.CreateInstruction((p, ct) => new OperationInstruction(p, operation, operand1.Result, operand2.Result, temp));
                            program.PushCompilerVariable(new ExpressionBlockInfo(temp, operand1.Start));
                        })),
                ]);

            UnresolvedTokenMatcher conditionMatcher = new("Condition");

            TokenMatcher boolFactorMatcher =
                "Bool Factor" |
                [
                    "Constant" % typeof(BoolConstantToken) | (async program =>
                    {
                        int start = program.CurrentInstructionIndex;
                        JumpInstruction jump = await program.CreateInstruction((p, ct) => new UnconditionalJumpInstruction(p, ct));

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
                    ] | (async program => {
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
                        return program.AddJumpInstructions(
                            (p, ct) => new ComparisonJumpInstruction(p, operation, operand1.Result, operand2.Result, ct),
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
                        ] | (async program => {
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
                        ] | (async program => {
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
                    "Constant" % typeof(ConstantToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(program.PopCompilerVariable<uint>()))),
                    "Symbol or Label" % typeof(IdentifierToken) | (async program => {
                        string name = program.PopCompilerVariable<string>();
                        Symbol symbol = program.GetOrAddAccessibleSymbol<Symbol>(name, () => new Label(name, []));
                        program.PushCompilerVariable(new InstructionFactory.Argument(symbol));
                    }),
                    "Underscore" % typeof(UnderscoreToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(null))),
                    "CV" % typeof(CVToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.In))),
                    "Ref" % typeof(RefToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.InOut))),
                    "Ret" % typeof(RetToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.Out))),
                ];

            ILInstruction.Resolve(
                "Intermediate Language Instruction" %
                [
                    "Label" ^
                        "Label" %
                        [
                            "Label Name" % typeof(IdentifierToken),
                            "Colon" % typeof(ColonToken),
                        ] | (async program => program.SetLabel(program.PopCompilerVariable<string>())),
                    "Instruction" ^
                    [
                        "Valid Opcode" |
                        [
                            "Assignment Token" % typeof(AssignmentToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Assignment))),
                            "In Token" % typeof(InToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Input))),
                            "Out Token" % typeof(OutToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Output))),
                            "Halt Token" % typeof(HaltToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Halt))),
                            "Jump Token" % typeof(JumpToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Jump))),
                            "Par Token" % typeof(ParToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Parameter))),
                            "Call Token" % typeof(CallToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Call))),
                            "Retv Token" % typeof(RetvToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Return))),
                            "Operation" % typeof(OperatorToken) | (async program => program.PushCompilerVariable(new InstructionFactory.Argument(program.PopCompilerVariable<OperationType>()))),
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
                        return program.AddIntermediateLanguageInstruction(arg0, arg1, arg2, arg3);
                    })
                ]);
            
            UnresolvedTokenMatcher blockMatcher = new("Block");

            statementMatcher.Resolve(
                "Statement" |
                [
                    blockMatcher,
                    "Assignment" %
                    [
                        "Variable ID" % typeof(IdentifierToken),
                        "Assignment Token" % typeof(AssignmentToken),
                        expressionMatcher,
                    ] | (async program => {
                        ExpressionBlockInfo expression = program.PopCompilerVariable<ExpressionBlockInfo>();
                        Variable variable = program.GetAccessibleSymbol<Variable>(program.PopCompilerVariable<string>());
                        // Assume that the variable is initialised when the first assignment is encountered.
                        // It is possible that this assignment will be skipped, but it's hard to check initisation in a way that accounts for jumps.
                        // This assumption doesn't lead to false positives and catches the most obvious (but not uncommon) true positives.
                        await program.CreateInstruction((p, ct) => new AssignmentInstruction(p, variable, expression.Result));
                        program.InitialiseVariable(variable);
                    }),
                    "If" %
                    [
                        "\"if\" Keyword" % typeof(IfToken),
                        conditionMatcher,
                        -"$true target" | (async program => {
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
                                -"$false target" | (async program => {
                                    int ifExit = program.CurrentInstructionIndex;
                                    JumpInstruction exitJump = await program.CreateInstruction((p, ct) => new UnconditionalJumpInstruction(p, ct));

                                    JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                    info.FalseOriginList.Targets = program.CurrentInstructionIndex;

                                    program.PushCompilerVariable(new JumpBlockInfo([], [exitJump], ifExit));
                                }),
                                "Else Body" ^ statementMatcher,
                            ],
                        -"$end" | (async program => {
                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                        }),
                    ],
                    "Switch Case" %
                    [
                        "\"switchcase\" Keyword" % typeof(SwitchCaseToken),
                        -"$start" | (async program => {
                            List<JumpInstruction> exitJumps = [];
                            program.PushCompilerVariable(exitJumps);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (async program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (async program => {
                                JumpInstruction exitJump = await program.CreateInstruction((p, ct) => new UnconditionalJumpInstruction(p, ct));

                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;

                                List<JumpInstruction> exitJumps = program.PeekCompilerVariable<List<JumpInstruction>>();
                                exitJumps.Add(exitJump);
                            }),
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(ColonToken),
                        "Default Body" ^ statementMatcher,
                        -"$end" | (async program => {
                            List<JumpInstruction> exitJumps = program.PopCompilerVariable<List<JumpInstruction>>();
                            exitJumps.Targets = program.CurrentInstructionIndex;
                        }),
                    ],
                    "While" %
                    [
                        "\"while\" Keyword" % typeof(WhileToken) | (async program => program.SetRepeatPoint()),
                        conditionMatcher,
                        -"$true target" | (async program => {
                            JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                        }),
                        "While Body" ^ statementMatcher,
                        -"$repeat" | (async program => {
                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            await program.CreateInstruction((p, ct) => new UnconditionalJumpInstruction(p, ct) { Target = info.Start });
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
                        -"$end" | (async program => program.SetBreakPoint()),
                    ],
                    "While Case" %
                    [
                        "\"whilecase\" Keyword" % typeof(WhileCaseToken) | (async program => program.SetRepeatPoint()),
                        -"$start" | (async program => {
                            int start = program.CurrentInstructionIndex;
                            program.PushCompilerVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (async program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$repeat & false target" | (async program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                int start = program.PeekCompilerVariable<int>();
                                await program.CreateInstruction((p, ct) => new UnconditionalJumpInstruction(p, ct) { Target = start });
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        -"$remove start variable" | (async program => _ = program.PopCompilerVariable<int>()),
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(ColonToken),
                        "Default Body" ^ statementMatcher,
                        -"$end" | (async program => program.SetBreakPoint()),
                    ],
                    "Until Case" %
                    [
                        "\"untilcase\" Keyword" % typeof(UntilCaseToken) | (async program => program.SetRepeatPoint()),
                        -"$start" | (async program => {
                            int start = program.CurrentInstructionIndex;
                            program.PushCompilerVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (async program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (async program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        "\"until\" Keyword" % typeof(UntilToken),
                        conditionMatcher,
                        -"$until" | (async program => {
                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = program.CurrentInstructionIndex;

                            int start = program.PopCompilerVariable<int>();
                            info.FalseOriginList.Targets = start;

                            program.SetBreakPoint();
                        }),
                    ],
                    "In Case" %
                    [
                        "\"incase\" Keyword" % typeof(InCaseToken) | (async program => program.SetRepeatPoint()),
                        -"$initialize" | (async program => {
                            int start = program.CurrentInstructionIndex;
                            program.PushCompilerVariable(start);

                            Variable exitFlag = program.GenerateTemp();
                            program.PushCompilerVariable(exitFlag);
                            await program.CreateInstruction((p, ct) => new AssignmentInstruction(p, exitFlag, 0));
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (async program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole incase
                            -"$false target" | (async program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                Variable exitFlag = program.PeekCompilerVariable<Variable>();

                                await program.CreateInstruction((p, ct) => new AssignmentInstruction(p, exitFlag, 1));
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        -"$end" | (async program =>
                        {
                            Variable exitFlag = program.PopCompilerVariable<Variable>();
                            int start = program.PopCompilerVariable<int>();
                            await program.CreateInstruction((p, ct) => new ComparisonJumpInstruction(p, OperationType.NotEqualTo, exitFlag, 0, ct) { Target = start });

                            program.SetBreakPoint();
                        }),
                    ],
                    "For Case" %
                    [
                        "\"forcase\" Keyword" % typeof(ForCaseToken) | (async program => program.SetRepeatPoint()),
                        "Iteration Identifier" % typeof(IdentifierToken),
                        -"$initialize" | (async program => {
                            string iterationVariableName = program.PopCompilerVariable<string>();
                            Variable iterationVariable = program.GetOrAddAccessibleSymbol(iterationVariableName,
                                () => new Variable(iterationVariableName, false, false));
                            program.InitialiseVariable(iterationVariable);
                            program.PushCompilerVariable(iterationVariable);
                            await program.CreateInstruction((p, ct) => new AssignmentInstruction(p, iterationVariable, 0));
                        }),
                        "Equals Sign" % OperationType.EqualTo | (async program => {
                            _ = program.PopCompilerVariable<OperationType>(); // Ignore Variable; it will always be an Equals Sign.
                        }),
                        expressionMatcher,
                        -"$condition & increment" | (async program =>
                        {
                            // Condition

                            ExpressionBlockInfo count = program.PopCompilerVariable<ExpressionBlockInfo>();
                            Variable iterationVariable = program.PopCompilerVariable<Variable>();
                            await program.AddJumpInstructions(
                                (p, ct) => new ComparisonJumpInstruction(p, OperationType.LessThan, iterationVariable, count.Result, ct),
                                program.CurrentInstructionIndex);

                            JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = program.CurrentInstructionIndex;

                            // Increment

                            await program.CreateInstruction((p, ct) => new OperationInstruction(p, OperationType.Add, iterationVariable, 1, iterationVariable));

                            program.SetRepeatPoint();
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (async program => {
                                JumpBlockInfo info = program.PeekCompilerVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole forcase
                            -"$false target" | (async program => {
                                JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = program.CurrentInstructionIndex;
                            }),
                        ],
                        -"$end" | (async program => {
                            program.SetBreakPoint();

                            JumpBlockInfo info = program.PopCompilerVariable<JumpBlockInfo>();
                            await program.CreateInstruction((p, ct) => new UnconditionalJumpInstruction(p, ct) { Target = info.Start });
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
                    ] | (program => program.CreateInstruction((p, ct) => new ReturnInstruction(p, program.PopCompilerVariable < ExpressionBlockInfo >().Result))),
                    "Input" %
                    [
                        "\"input\" Keyword" % typeof(InputToken),
                        "Variable ID" % typeof(IdentifierToken),
                    ] | (async program => {
                        Variable variable = program.GetAccessibleSymbol<Variable>(program.PopCompilerVariable<string>());
                        await program.CreateInstruction((p, ct) => new InputInstruction(p, variable));
                        program.InitialiseVariable(variable);
                    }),
                    "Print" %
                    [
                        "\"print\" Keyword" % typeof(PrintToken),
                        expressionMatcher,
                    ] | (program => program.CreateInstruction((p, ct) => new OutputInstruction(p, program.PopCompilerVariable < ExpressionBlockInfo >().Result))),
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

            blockMatcher.Resolve(
                "Block" >>
                    "Block Contents" %
                    [
                        -"$start" | (async program => {
                            await program.EnterScope();
                            program.SetRepeatPoint();
                        }),
                        declarationsMatcher,
                        functionsMatcher,
                        statementsMatcher,
                        -"$end" | (async program => {
                            program.ExitScope();
                            program.SetBreakPoint();
                        }),
                    ]);

            superMatcher =
                "Program" %
                [
                    "\"program\" Keyword" % typeof(ProgramToken),
                    "Program ID" % typeof(IdentifierToken) | (async program =>
                    {
                        await program.CreateFunction(program.PopCompilerVariable<string>(), true);
                        program.AddReturnValueParameter();
                    }),
                    "Program Body" % statementMatcher,
                    "EOF" % typeof(EOFToken) | (program => program.FinalizeFunction((p, ct) => new HaltInstruction(p))),
                ];
        }

        public async Task Analyse(Stream<Token> input, IntermediateProgram? output = null, CancellationToken? cancellationToken = null)
        {
            try
            {
                var tokens = input.GetAsyncEnumerable().GetAsyncEnumerator();
                if (!await tokens.MoveNextAsync()) throw new SyntaxAnalyserException(default, $"Expected EOF Token.");
                if (await superMatcher.TryMatch(tokens, output) == false)
                    throw new SyntaxAnalyserException(tokens.Current.Position, $"Expected {superMatcher.Name}, but got {tokens.Current}.");
                output?.CheckNoLeftoverVariables();
            }
            finally
            {
                output?.CompleteAdding();
            }
        }
    }
}