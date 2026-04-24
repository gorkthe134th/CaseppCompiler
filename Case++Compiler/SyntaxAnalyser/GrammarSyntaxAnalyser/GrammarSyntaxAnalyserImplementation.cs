using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Instructions;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.Symbols;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

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
                            "Variable ID" % typeof(IdentifierToken) | (p => {
                                p.AddSymbol(new VariableSymbol(p.PopVariable<string>(), false));
                            }),
                            "More Variables" *
                            [
                                "Comma" % typeof(CommaToken),
                                "Variable ID" % typeof(IdentifierToken) | (p => {
                                    p.AddSymbol(new VariableSymbol(p.PopVariable<string>(), false));
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
                    ] | (p => {
                        string id = p.PopVariable<string>();
                        p.PeekVariable<FunctionSymbol>().AddParameter(new TypeRestrictedFormalParameter<InParameter>());
                        p.AddSymbol(new VariableSymbol(id, false) { IsInitialised = true });
                    }),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => {
                        string id = p.PopVariable<string>();
                        p.PeekVariable<FunctionSymbol>().AddParameter(new TypeRestrictedFormalParameter<OutParameter>());
                        p.AddSymbol(new VariableSymbol(id, true) { IsInitialised = false });
                    }),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => {
                        string id = p.PopVariable<string>();
                        p.PeekVariable<FunctionSymbol>().AddParameter(new TypeRestrictedFormalParameter<InOutParameter>());
                        p.AddSymbol(new VariableSymbol(id, true) { IsInitialised = true });
                    }),
                ];

            UnresolvedTokenMatcher statementMatcher = new("Statement");

            TokenMatcher functionsMatcher =
                "Functions" *
                (
                    "Function" %
                    [
                        "\"function\" Keyword" % typeof(FunctionToken),
                        "Function ID" % typeof(IdentifierToken) | (p => p.CreateFunction(p.PopVariable<string>())),
                        "Formal Parameter List" > (
                            "Formal Parameters" ^
                            [
                                formalParameterMatcher,
                                "More Parameters" *
                                [
                                    "Comma" % typeof(CommaToken),
                                    formalParameterMatcher,
                                ],
                            ]) | (p => _ = p.PopVariable<FunctionSymbol>() /* Remove FunctionSymbol from stack. It no longer needs to be modified. */),
                        "Function Body" % statementMatcher,
                        "Optional Semi Colon" ^ typeof(SemiColonToken),
                    ] | (p => p.FinalizeFunction((l, c) => new ReturnInstruction(l, c, 0)))
                );

            UnresolvedTokenMatcher expressionMatcher = new("Expression");

            static bool CheckInitialisation(VariableSymbol symbol, [NotNullWhen(false)] out string? errorMessage)
            {
                if (!symbol.IsInitialised)
                {
                    errorMessage = $"Use of uninitialised variable {symbol.Name}";
                    return false;
                }
                errorMessage = null;
                return true;
            }

            TokenMatcher actualParameterMatcher =
                "Actual Parameter" |
                [
                    "In Parameter" %
                    [
                        "Optional Keyword" ^
                            "\"in\" Keyword" % typeof(InToken),
                        expressionMatcher,
                    ] | (p => p.AddParameterToCallBlock(new InParameter(p.PopVariable<ExpressionBlockInfo>().Result))),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => p.AddParameterToCallBlock(new OutParameter(p.GetSymbolInScope<VariableSymbol>(p.PopVariable<string>())))),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => p.AddParameterToCallBlock(new InOutParameter(p.GetSymbolInScope<VariableSymbol>(p.PopVariable<string>(), CheckInitialisation)))),
                ];

            TokenMatcher factorMatcher =
                "Factor" |
                [
                    "Constant" % typeof(ConstantToken) | (p => p.PushVariable(new ExpressionBlockInfo(p.PopVariable<uint>(), p.CurrentPosition))),
                    "Sub-Expression" > expressionMatcher,
                    "Identifier" %
                    [
                        "Name" % typeof(IdentifierToken),
                        "Optional Parameters" |
                        [
                            "Actual Parameter List" >
                            [
                                -"$function" | (p =>
                                    p.PushVariable(new FunctionCallBlockInfo(p.GetSymbolInScope<FunctionSymbol>(p.PopVariable<string>()), p.CurrentPosition))),
                                "Actual Parameters" ^
                                [
                                    actualParameterMatcher,
                                    "More Parameters" *
                                    [
                                        "Comma" % typeof(CommaToken),
                                        actualParameterMatcher,
                                    ],
                                ],
                                -"$call" | (p => {
                                    FunctionCallBlockInfo callBlockInfo = p.PopVariable<FunctionCallBlockInfo>();
                                    VariableSymbol result = p.GenerateTemp();
                                    foreach (var parameter in callBlockInfo.Parameters)
                                        p.CreateInstruction((l, c) => new ParameterInstruction(l, c, parameter));
                                    p.CreateInstruction((l, c) => new ParameterInstruction(l, c, new OutParameter(result)));
                                    p.CreateInstruction((l, c) => new CallInstruction(l, c, callBlockInfo.FunctionSymbol));
                                    p.PushVariable(new ExpressionBlockInfo(result, callBlockInfo.Start));
                                }),
                            ],
                            -"$variable" | (p => {
                                // If there are no parameters, expect an initialised variable
                                VariableSymbol variable = p.GetSymbolInScope<VariableSymbol>(p.PopVariable<string>(), CheckInitialisation);
                                p.PushVariable(new ExpressionBlockInfo(variable, p.CurrentPosition));
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
                                "Multiply" % OperatorToken.OperationType.Multiply,
                                "Divide" % OperatorToken.OperationType.Divide,
                            ],
                            factorMatcher,
                        ] | (p => {
                            ExpressionBlockInfo operand2 = p.PopVariable<ExpressionBlockInfo>();
                            OperatorToken.OperationType operation = p.PopVariable<OperatorToken.OperationType>();
                            ExpressionBlockInfo operand1 = p.PopVariable<ExpressionBlockInfo>();
                            VariableSymbol temp = p.GenerateTemp();
                            p.CreateInstruction((l, c) => new OperationInstruction(l, c, operation, operand1.Result, operand2.Result, temp));
                            p.PushVariable(new ExpressionBlockInfo(temp, operand1.Start));
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
                                "Plus" % OperatorToken.OperationType.Add,
                                "Minus" % OperatorToken.OperationType.Subtract,
                            ],
                            termMatcher,
                            -"$apply sign" | (p => {
                                ExpressionBlockInfo term = p.PopVariable<ExpressionBlockInfo>();
                                OperatorToken.OperationType sign = p.PopVariable<OperatorToken.OperationType>();
                                VariableSymbol temp = p.GenerateTemp();
                                p.CreateInstruction((l, c) => new OperationInstruction(l, c, sign, 0, term.Result, temp));
                                p.PushVariable(new ExpressionBlockInfo(temp, term.Start));
                            }),
                        ],
                        "Unsigned Term" % termMatcher,
                    ],
                    "More Terms" * (
                        "Operation" %
                        [
                            "Additive Operation" |
                            [
                                "Add" % OperatorToken.OperationType.Add,
                                "Subtract" % OperatorToken.OperationType.Subtract,
                            ],
                            termMatcher,
                        ] | (p => {
                            ExpressionBlockInfo operand2 = p.PopVariable<ExpressionBlockInfo>();
                            OperatorToken.OperationType operation = p.PopVariable<OperatorToken.OperationType>();
                            ExpressionBlockInfo operand1 = p.PopVariable<ExpressionBlockInfo>();
                            VariableSymbol temp = p.GenerateTemp();
                            p.CreateInstruction((l, c) => new OperationInstruction(l, c, operation, operand1.Result, operand2.Result, temp));
                            p.PushVariable(new ExpressionBlockInfo(temp, operand1.Start));
                        })),
                ]);

            UnresolvedTokenMatcher conditionMatcher = new("Condition");

            TokenMatcher boolFactorMatcher =
                "Bool Factor" |
                [
                    "Constant" % typeof(BoolConstantToken) | (p =>
                    {
                        int start = p.CurrentPosition;
                        JumpInstruction jump = p.CreateInstruction((l, c) => new UnconditionalJumpInstruction(l, c, null));

                        bool @bool = p.PopVariable<bool>();
                        List<JumpInstruction> trueOriginList  = [];
                        List<JumpInstruction> falseOriginList = [];
                        if (@bool == true) trueOriginList.Add(jump);
                        else              falseOriginList.Add(jump);
                        p.PushVariable(new JumpBlockInfo(trueOriginList, falseOriginList, start));
                    }),
                    "Inverted Sub-Condition" %
                    [
                        "\"not\" Keyword" % OperatorToken.OperationType.Not,
                        "Sub-Condition" >= conditionMatcher,
                    ] | (p => {
                        JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                        _ = p.PopVariable<OperatorToken.OperationType>(); // Ignore Variable; it will always be Not.
                        p.PushVariable(new JumpBlockInfo(info.FalseOriginList, info.TrueOriginList, info.Start));
                    }),
                    "Sub-Condition" >= conditionMatcher,
                    "Comparison" %
                    [
                        expressionMatcher,
                        "Comparison Operation" |
                        [
                            "Equality" % OperatorToken.OperationType.EqualTo,
                            "Less Than" % OperatorToken.OperationType.LessThan,
                            "Greater Than" % OperatorToken.OperationType.GreaterThan,
                            "Inequality" % OperatorToken.OperationType.NotEqualTo,
                            "Less Than or Equal To" % OperatorToken.OperationType.LessThanOrEqualTo,
                            "Greater Than or Equal To" % OperatorToken.OperationType.GreaterThanOrEqualTo,
                        ],
                        expressionMatcher,
                    ] | (p => {
                        ExpressionBlockInfo operand2 = p.PopVariable<ExpressionBlockInfo>();
                        OperatorToken.OperationType operation = p.PopVariable<OperatorToken.OperationType>();
                        ExpressionBlockInfo operand1 = p.PopVariable<ExpressionBlockInfo>();
                        p.AddJumpInstructions(
                            (l, c) => new ComparisonJumpInstruction(l, c, operation, operand1.Result, operand2.Result, null),
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
                            "And" % OperatorToken.OperationType.And,
                            boolFactorMatcher,
                        ] | (p => {
                            JumpBlockInfo info2 = p.PopVariable<JumpBlockInfo>();
                            _ = p.PopVariable<OperatorToken.OperationType>(); // Ignore Variable; it will always be And.
                            JumpBlockInfo info1 = p.PopVariable<JumpBlockInfo>();

                            info1.TrueOriginList.Targets = info2.Start;
                            info1.FalseOriginList.AddRange(info2.FalseOriginList);
                            p.PushVariable(new JumpBlockInfo(info2.TrueOriginList, info1.FalseOriginList, info1.Start));
                        })),
                ];

            conditionMatcher.Resolve(
                "Condition" %
                [
                    boolTermMatcher,
                    "More Bool Terms" * (
                        "Operation" %
                        [
                            "Or" % OperatorToken.OperationType.Or,
                            boolTermMatcher,
                        ] | (p => {
                            JumpBlockInfo info2 = p.PopVariable<JumpBlockInfo>();
                            _ = p.PopVariable<OperatorToken.OperationType>(); // Ignore Variable; it will always be Or.
                            JumpBlockInfo info1 = p.PopVariable<JumpBlockInfo>();

                            info1.FalseOriginList.Targets = info2.Start;
                            info1.TrueOriginList.AddRange(info2.TrueOriginList);
                            p.PushVariable(new JumpBlockInfo(info1.TrueOriginList, info2.FalseOriginList, info1.Start));
                        })),
                ]);

            UnresolvedTokenMatcher ILInstruction = new("IL Instruction");

            TokenMatcher ILArgumentMatcher =
                "Argument" |
                [
                    "Constant" % typeof(ConstantToken) | (p => p.PushVariable(new InstructionFactory.Argument(p.PopVariable<uint>()))),
                    "Symbol or Label" % typeof(IdentifierToken) | (p => {
                        string name = p.PopVariable<string>();
                        if (!p.TryGetSymbolInScope<Symbol>(name, out var symbol))
                        {
                            symbol = new LabelSymbol(name, []);
                            p.AddSymbol(symbol);
                        }
                        p.PushVariable(new InstructionFactory.Argument(symbol));
                    }),
                    "Underscore" % typeof(UnderscoreToken) | (p => p.PushVariable(new InstructionFactory.Argument(null))),
                    "CV" % typeof(CVToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.In))),
                    "Ref" % typeof(RefToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.InOut))),
                    "Ret" % typeof(RetToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.ParameterType.Out))),
                ];

            ILInstruction.Resolve(
                "Intermediate Language Instruction" %
                [
                    "Label" ^
                        "Label" %
                        [
                            "Label Name" % typeof(IdentifierToken),
                            "Colon" % typeof(ColonToken),
                        ] | (p => p.SetLabel(p.PopVariable<string>())),
                    "Instruction" ^
                    [
                        "Valid Opcode" |
                        [
                            "Assignment Token" % typeof(AssignmentToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Assignment))),
                            "In Token" % typeof(InToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Input))),
                            "Out Token" % typeof(OutToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Output))),
                            "Halt Token" % typeof(HaltToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Halt))),
                            "Jump Token" % typeof(JumpToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Jump))),
                            "Par Token" % typeof(ParToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Parameter))),
                            "Call Token" % typeof(CallToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Call))),
                            "Retv Token" % typeof(RetvToken) | (p => p.PushVariable(new InstructionFactory.Argument(InstructionFactory.Opcode.Return))),
                            "Operation" % typeof(OperatorToken) | (p => p.PushVariable(new InstructionFactory.Argument(p.PopVariable<OperatorToken.OperationType>()))),
                        ],
                        "Comma" % typeof(CommaToken),
                        ILArgumentMatcher,
                        "Comma" % typeof(CommaToken),
                        ILArgumentMatcher,
                        "Comma" % typeof(CommaToken),
                        ILArgumentMatcher,
                    ] | (p => {
                        InstructionFactory.Argument arg3 = p.PopVariable<InstructionFactory.Argument>();
                        InstructionFactory.Argument arg2 = p.PopVariable<InstructionFactory.Argument>();
                        InstructionFactory.Argument arg1 = p.PopVariable<InstructionFactory.Argument>();
                        InstructionFactory.Argument arg0 = p.PopVariable<InstructionFactory.Argument>();
                        p.AddIntermediateLanguageInstruction(arg0, arg1, arg2, arg3);
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
                    ] | (p => {
                        ExpressionBlockInfo expression = p.PopVariable<ExpressionBlockInfo>();
                        VariableSymbol variable = p.GetSymbolInScope<VariableSymbol>(p.PopVariable<string>());
                        // Assume that the variable is initialised when the first assignment is encountered.
                        // It is possible that this assignment will be skipped, but it's hard to check initisation in a way that accounts for jumps.
                        // This assumption doesn't lead to false positives and catches the most obvious (but not uncommon) true positives.
                        variable.Initialise();
                        p.CreateInstruction((l, c) => new AssignmentInstruction(l, c, variable, expression.Result));
                    }),
                    "If" %
                    [
                        "\"if\" Keyword" % typeof(IfToken),
                        conditionMatcher,
                        -"$true target" | (p => {
                            JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = p.CurrentPosition;
                        }),
                        "If Body" ^ statementMatcher,
                        "Optional Else" ^
                            "Else" %
                            [
                                // "Optional Semi Colon" ^ typeof(SemiColonToken),
                                // Cannot allow a semi colon here because it's ambiguous
                                // whether a semi colon ends the main body or the whole if
                                "\"else\" Keyword" % typeof(ElseToken),
                                -"$false target" | (p => {
                                    int ifExit = p.CurrentPosition;
                                    JumpInstruction exitJump = p.CreateInstruction((l, c) => new UnconditionalJumpInstruction(l, c, null));

                                    JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                                    info.FalseOriginList.Targets = p.CurrentPosition;

                                    p.PushVariable(new JumpBlockInfo([], [exitJump], ifExit));
                                }),
                                "Else Body" ^ statementMatcher,
                            ],
                        -"$end" | (p => {
                            JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                            info.FalseOriginList.Targets = p.CurrentPosition;
                        }),
                    ],
                    "Switch Case" %
                    [
                        "\"switchcase\" Keyword" % typeof(SwitchCaseToken),
                        -"$start" | (p => {
                            List<JumpInstruction> exitJumps = [];
                            p.PushVariable(exitJumps);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = p.CurrentPosition;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (p => {
                                JumpInstruction exitJump = p.CreateInstruction((l, c) => new UnconditionalJumpInstruction(l, c, null));

                                JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = p.CurrentPosition;

                                List<JumpInstruction> exitJumps = p.PeekVariable<List<JumpInstruction>>();
                                exitJumps.Add(exitJump);
                            }),
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(ColonToken),
                        "Default Body" ^ statementMatcher,
                        -"$end" | (p => {
                            List<JumpInstruction> exitJumps = p.PopVariable<List<JumpInstruction>>();
                            exitJumps.Targets = p.CurrentPosition;
                        }),
                    ],
                    "While" %
                    [
                        "\"while\" Keyword" % typeof(WhileToken) | (p => p.SetRepeatPoint()),
                        conditionMatcher,
                        -"$true target" | (p => {
                            JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = p.CurrentPosition;
                        }),
                        "While Body" ^ statementMatcher,
                        -"$repeat" | (p => {
                            JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                            p.CreateInstruction((l, c) => new UnconditionalJumpInstruction(l, c, info.Start));
                            info.FalseOriginList.Targets = p.CurrentPosition;
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
                        -"$end" | (p => p.SetBreakPoint()),
                    ],
                    "While Case" %
                    [
                        "\"whilecase\" Keyword" % typeof(WhileCaseToken) | (p => p.SetRepeatPoint()),
                        -"$start" | (p => {
                            int start = p.CurrentPosition;
                            p.PushVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = p.CurrentPosition;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$repeat & false target" | (p => {
                                JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                                int start = p.PeekVariable<int>();
                                p.CreateInstruction((l, c) => new UnconditionalJumpInstruction(l, c, start));
                                info.FalseOriginList.Targets = p.CurrentPosition;
                            }),
                        ],
                        -"$remove start variable" | (p => _ = p.PopVariable<int>()),
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(ColonToken),
                        "Default Body" ^ statementMatcher,
                        -"$end" | (p => p.SetBreakPoint()),
                    ],
                    "Until Case" %
                    [
                        "\"untilcase\" Keyword" % typeof(UntilCaseToken) | (p => p.SetRepeatPoint()),
                        -"$start" | (p => {
                            int start = p.CurrentPosition;
                            p.PushVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = p.CurrentPosition;
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (p => {
                                JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = p.CurrentPosition;
                            }),
                        ],
                        "\"until\" Keyword" % typeof(UntilToken),
                        conditionMatcher,
                        -"$until" | (p => {
                            JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = p.CurrentPosition;

                            int start = p.PopVariable<int>();
                            info.FalseOriginList.Targets = start;

                            p.SetBreakPoint();
                        }),
                    ],
                    "In Case" %
                    [
                        "\"incase\" Keyword" % typeof(InCaseToken) | (p => p.SetRepeatPoint()),
                        -"$initialize" | (p => {
                            int start = p.CurrentPosition;
                            p.PushVariable(start);

                            VariableSymbol exitFlag = p.GenerateTemp();
                            p.PushVariable(exitFlag);
                            p.CreateInstruction((l, c) => new AssignmentInstruction(l, c, exitFlag, 0));
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = p.CurrentPosition;
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole incase
                            -"$false target" | (p => {
                                JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                                VariableSymbol exitFlag = p.PeekVariable<VariableSymbol>();

                                p.CreateInstruction((l, c) => new AssignmentInstruction(l, c, exitFlag, 1));
                                info.FalseOriginList.Targets = p.CurrentPosition;
                            }),
                        ],
                        -"$end" | (p =>
                        {
                            VariableSymbol exitFlag = p.PopVariable<VariableSymbol>();
                            int start = p.PopVariable<int>();
                            p.CreateInstruction((l, c) => new ComparisonJumpInstruction(l, c, OperatorToken.OperationType.NotEqualTo, exitFlag, 0, start));

                            p.SetBreakPoint();
                        }),
                    ],
                    "For Case" %
                    [
                        "\"forcase\" Keyword" % typeof(ForCaseToken) | (p => p.SetRepeatPoint()),
                        "Iteration Identifier" % typeof(IdentifierToken),
                        -"$initialize" | (p => {
                            VariableSymbol iterationVariable = new(p.PopVariable<string>(), true);
                            p.AddSymbol(iterationVariable);
                            p.PushVariable(iterationVariable);
                            p.CreateInstruction((l, c) => new AssignmentInstruction(l, c, iterationVariable, 0));
                        }),
                        "Equals Sign" % OperatorToken.OperationType.EqualTo | (p => {
                            _ = p.PopVariable<OperatorToken.OperationType>(); // Ignore Variable; it will always be an Equals Sign.
                        }),
                        expressionMatcher,
                        -"$condition & increment" | (p =>
                        {
                            // Condition

                            ExpressionBlockInfo count = p.PopVariable<ExpressionBlockInfo>();
                            VariableSymbol iterationVariable = p.PopVariable<VariableSymbol>();
                            p.AddJumpInstructions(
                                (l, c) => new ComparisonJumpInstruction(l, c, OperatorToken.OperationType.LessThan, iterationVariable, count.Result, null),
                                p.CurrentPosition);

                            JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                            info.TrueOriginList.Targets = p.CurrentPosition;

                            // Increment

                            p.CreateInstruction((l, c) => new OperationInstruction(l, c, OperatorToken.OperationType.Add, iterationVariable, 1, iterationVariable));

                            p.SetRepeatPoint();
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(ColonToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = p.PeekVariable<JumpBlockInfo>();
                                info.TrueOriginList.Targets = p.CurrentPosition;
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole forcase
                            -"$false target" | (p => {
                                JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                                info.FalseOriginList.Targets = p.CurrentPosition;
                            }),
                        ],
                        -"$end" | (p => {
                            p.SetBreakPoint();

                            JumpBlockInfo info = p.PopVariable<JumpBlockInfo>();
                            p.CreateInstruction((l, c) => new UnconditionalJumpInstruction(l, c, info.Start));
                            info.FalseOriginList.Targets = p.CurrentPosition;

                            p.SetBreakPoint();
                        }),
                    ],
                    "Break" %
                    [
                        "\"break\" Keyword" % typeof(BreakToken),
                        "Break Count" % typeof(ConstantToken),
                    ] | (p => p.AddBreakInstruction(p.PopVariable<uint>())),
                    "Repeat" %
                    [
                        "\"repeat\" Keyword" % typeof(RepeatToken),
                        "Repeat Index" % typeof(ConstantToken),
                    ] | (p => p.AddRepeatInstruction(p.PopVariable<uint>())),
                    "Return" %
                    [
                        "\"return\" Keyword" % typeof(ReturnToken),
                        expressionMatcher,
                    ] | (p => p.CreateInstruction((l, c) => new ReturnInstruction(l, c, p.PopVariable<ExpressionBlockInfo>().Result))),
                    "Input" %
                    [
                        "\"input\" Keyword" % typeof(InputToken),
                        "Variable ID" % typeof(IdentifierToken),
                    ] | (p => {
                        VariableSymbol variable = p.GetSymbolInScope<VariableSymbol>(p.PopVariable<string>());
                        p.CreateInstruction((l, c) => new InputInstruction(l, c, variable));
                    }),
                    "Print" %
                    [
                        "\"print\" Keyword" % typeof(PrintToken),
                        expressionMatcher,
                    ] | (p => p.CreateInstruction((l, c) => new OutputInstruction(l, c, p.PopVariable<ExpressionBlockInfo>().Result))),
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
                        -"$start" | (p => {
                            p.EnterScope();
                            p.SetRepeatPoint();
                        }),
                        declarationsMatcher,
                        functionsMatcher,
                        statementsMatcher,
                        -"$end" | (p => {
                            p.ExitScope();
                            p.SetBreakPoint();
                        }),
                    ]);

            superMatcher =
                "Program" %
                [
                    "\"program\" Keyword" % typeof(ProgramToken),
                    "Program ID" % typeof(IdentifierToken) | (p => {
                        p.CreateFunction(p.PopVariable<string>());
                        _ = p.PopVariable<FunctionSymbol>(); // Ignore Symbol; program has no arguments to keep track of.
                    }),
                    "Program Body" % statementMatcher,
                    "EOF" % typeof(EOFToken) | (p => p.FinalizeFunction((l, c) => new HaltInstruction(l, c))),
                ];
        }

        public void Analyse(BlockingCollection<Token> input, IntermediateProgram? output = null)
        {
            var tokens = input.GetConsumingEnumerable().GetEnumerator();
            TokenMatcher.MoveNext(tokens);
            if (superMatcher.TryMatch(tokens, output) == false)
                throw new SyntaxAnalyserException($"Expected {superMatcher.Name}: {tokens.Current}");
            output?.CheckNoLeftoverVariables();
        }
    }
}