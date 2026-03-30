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
                            "Variable ID" % typeof(IdentifierToken) | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
                            "More Variables" *
                            [
                                "Comma" % typeof(CommaToken),
                                "Variable ID" % typeof(IdentifierToken) | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
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
                    ] | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
                ];

            UnresolvedTokenMatcher statementMatcher = new("Statement");

            TokenMatcher functionsMatcher =
                "Functions" *
                (
                    "Function" %
                    [
                        "\"function\" Keyword" % typeof(FunctionToken) | (p => p.CreateFunction()),
                        "Function ID" % typeof(IdentifierToken) | (p => p.CurrentFunction.Name = (string)p.PopVariable()),
                        "Formal Parameter List" > (
                            "Formal Parameters" ^
                            [
                                formalParameterMatcher,
                                "More Parameters" *
                                [
                                    "Comma" % typeof(CommaToken),
                                    formalParameterMatcher,
                                ],
                            ]),
                        "Function Body" % statementMatcher,
                        "Optional Semi Colon" ^ typeof(SemiColonToken),
                    ] | (p => p.FinalizeFunction(typeof(ReturnInstruction), [typeof(object)], [0]))
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
                    ] | (p => {
                        ExpressionBlockInfo info = (ExpressionBlockInfo)p.PopVariable();
                        var parameters = (List<(object, ParameterInstruction.ParameterType)>)p.PeekVariable();
                        parameters.Add((info.Result, ParameterInstruction.ParameterType.In));
                    }),
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => {
                        object result = p.PopVariable();
                        var parameters = (List<(object, ParameterInstruction.ParameterType)>)p.PeekVariable();
                        parameters.Add((result, ParameterInstruction.ParameterType.Out));
                    }),
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ] | (p => {
                        object result = p.PopVariable();
                        var parameters = (List<(object, ParameterInstruction.ParameterType)>)p.PeekVariable();
                        parameters.Add((result, ParameterInstruction.ParameterType.InOut));
                    }),
                ];

            TokenMatcher factorMatcher =
                "Factor" |
                [
                    "Constant" % typeof(ConstantToken) | (p =>
                        p.PushVariable(new ExpressionBlockInfo(p.PopVariable(), p.CurrentFunction.CurrentPosition))),
                    "Sub-Expression" > expressionMatcher,
                    "Identifier" %
                    [
                        "Name" % typeof(IdentifierToken) | (p =>
                            p.PushVariable(new ExpressionBlockInfo(p.PopVariable(), p.CurrentFunction.CurrentPosition))),
                        "Optional Parameters" ^
                            "Actual Parameter List" >
                            [
                                -"$list" | (p => p.PushVariable((List<(object, ParameterInstruction.ParameterType)>)[])),
                                "Actual Parameters" ^
                                [
                                    actualParameterMatcher,
                                    "More Parameters" *
                                    [
                                        "Comma" % typeof(CommaToken),
                                        actualParameterMatcher,
                                    ],
                                ],
                                -"call" | (p => {
                                    var parameters = (List<(object value, ParameterInstruction.ParameterType type)>)p.PopVariable();
                                    ExpressionBlockInfo info = (ExpressionBlockInfo)p.PopVariable();
                                    string result = p.GenerateTemp();
                                    foreach ((var parameter, var type) in parameters)
                                        p.AddInstruction(
                                            typeof(ParameterInstruction),
                                            [typeof(object), typeof(ParameterInstruction.ParameterType)],
                                            [parameter, type]);
                                    p.AddInstruction(
                                        typeof(ParameterInstruction),
                                        [typeof(object), typeof(ParameterInstruction.ParameterType)],
                                        [result, ParameterInstruction.ParameterType.Out]);
                                    p.AddInstruction(typeof(CallInstruction), [typeof(string)], [info.Result]);
                                    p.PushVariable(new ExpressionBlockInfo(result, info.Start));
                                }),
                            ],
                    ]
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
                            ExpressionBlockInfo operand2 = (ExpressionBlockInfo)p.PopVariable();
                            OperatorToken.OperationType operation = (OperatorToken.OperationType)p.PopVariable();
                            ExpressionBlockInfo operand1 = (ExpressionBlockInfo)p.PopVariable();
                            string temp = p.GenerateTemp();
                            p.AddInstruction(
                                typeof(OperationInstruction),
                                [typeof(OperatorToken.OperationType), typeof(object), typeof(object), typeof(string)],
                                [operation, operand1.Result, operand2.Result, temp]);
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
                                ExpressionBlockInfo term = (ExpressionBlockInfo)p.PopVariable();
                                OperatorToken.OperationType sign = (OperatorToken.OperationType)p.PopVariable();
                                string temp = p.GenerateTemp();
                                p.AddInstruction(
                                    typeof(OperationInstruction),
                                    [typeof(OperatorToken.OperationType), typeof(object), typeof(object), typeof(string)],
                                    [sign, 0, term.Result, temp]);
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
                            ExpressionBlockInfo operand2 = (ExpressionBlockInfo)p.PopVariable();
                            OperatorToken.OperationType operation = (OperatorToken.OperationType)p.PopVariable();
                            ExpressionBlockInfo operand1 = (ExpressionBlockInfo)p.PopVariable();
                            string temp = p.GenerateTemp();
                            p.AddInstruction(
                                typeof(OperationInstruction),
                                [typeof(OperatorToken.OperationType), typeof(object), typeof(object), typeof(string)],
                                [operation, operand1.Result, operand2.Result, temp]);
                            p.PushVariable(new ExpressionBlockInfo(temp, operand1.Start));
                        })),
                ]);

            UnresolvedTokenMatcher conditionMatcher = new("Condition");

            TokenMatcher boolFactorMatcher =
                "Bool Factor" |
                [
                    "Constant" % typeof(BoolConstantToken) | (p =>
                    {
                        int start = p.CurrentFunction.CurrentPosition;
                        p.AddInstruction(typeof(UnconditionalJumpInstruction), [], []);

                        bool @bool = (bool)p.PopVariable();
                        List<int> trueOriginList  = [];
                        List<int> falseOriginList = [];
                        if (@bool == true) trueOriginList.Add(start);
                        else              falseOriginList.Add(start);
                        p.PushVariable(new JumpBlockInfo(trueOriginList, falseOriginList, start));
                    }),
                    "Inverted Sub-Condition" %
                    [
                        "\"not\" Keyword" % OperatorToken.OperationType.Not,
                        "Sub-Condition" >= conditionMatcher,
                    ] | (p => {
                        JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                        _ = p.PopVariable(); // Ignore Variable; it will always be Not.
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
                        ExpressionBlockInfo operand2 = (ExpressionBlockInfo)p.PopVariable();
                        OperatorToken.OperationType operation = (OperatorToken.OperationType)p.PopVariable();
                        ExpressionBlockInfo operand1 = (ExpressionBlockInfo)p.PopVariable();
                        p.AddJumpInstructions(
                            typeof(ComparisonJumpInstruction),
                            [typeof(OperatorToken.OperationType), typeof(object), typeof(object)],
                            [operation, operand1.Result, operand2.Result],
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
                            JumpBlockInfo info2 = (JumpBlockInfo)p.PopVariable();
                            _ = p.PopVariable(); // Ignore Variable; it will always be And.
                            JumpBlockInfo info1 = (JumpBlockInfo)p.PopVariable();

                            p.CurrentFunction.SetJumpTargets(info1.TrueOriginList, info2.Start);
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
                            JumpBlockInfo info2 = (JumpBlockInfo)p.PopVariable();
                            _ = p.PopVariable(); // Ignore Variable; it will always be Or.
                            JumpBlockInfo info1 = (JumpBlockInfo)p.PopVariable();

                            p.CurrentFunction.SetJumpTargets(info1.FalseOriginList, info2.Start);
                            info1.TrueOriginList.AddRange(info2.TrueOriginList);
                            p.PushVariable(new JumpBlockInfo(info1.TrueOriginList, info2.FalseOriginList, info1.Start));
                        })),
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
                        ExpressionBlockInfo expression = (ExpressionBlockInfo)p.PopVariable();
                        string variableID = (string)p.PopVariable();
                        p.AddInstruction(
                            typeof(AssignmentInstruction),
                            [typeof(string), typeof(object)],
                            [variableID, expression.Result]);
                    }),
                    "If" %
                    [
                        "\"if\" Keyword" % typeof(IfToken),
                        conditionMatcher,
                        -"$true target" | (p => {
                            JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);
                            p.PushVariable(info);
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
                                    int ifExit = p.CurrentFunction.CurrentPosition;
                                    p.AddInstruction(typeof(UnconditionalJumpInstruction), [], []);

                                    JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                    p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);

                                    p.PushVariable(new JumpBlockInfo([], [ifExit], ifExit));
                                }),
                                "Else Body" ^ statementMatcher,
                            ],
                        -"$end" | (p => {
                            JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);
                        }),
                    ],
                    "Switch Case" %
                    [
                        "\"switchcase\" Keyword" % typeof(SwitchCaseToken),
                        -"$start" | (p => {
                            List<int> exitJumps = [];
                            p.PushVariable(exitJumps);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);
                                p.PushVariable(info);
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (p => {
                                int whenExit = p.CurrentFunction.CurrentPosition;
                                p.AddInstruction(typeof(UnconditionalJumpInstruction), [], []);

                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);

                                List<int> exitJumps = (List<int>)p.PopVariable();
                                exitJumps.Add(whenExit);
                                p.PushVariable(exitJumps);
                            }),
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(CaseStartToken),
                        "Default Body" ^ statementMatcher,
                        -"$end" | (p => {
                            List<int> exitJumps = (List<int>)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets(exitJumps, p.CurrentFunction.CurrentPosition);
                        }),
                    ],
                    "While" %
                    [
                        "\"while\" Keyword" % typeof(WhileToken) | (p => p.CurrentFunction.IncreaseAllBreaks(1)),
                        conditionMatcher,
                        -"$true target" | (p => {
                            JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);
                            p.PushVariable(info);
                        }),
                        "While Body" ^ statementMatcher,
                        -"repeat" | (p => {
                            int end = p.CurrentFunction.CurrentPosition;
                            p.AddInstruction(typeof(UnconditionalJumpInstruction), [], []);

                            JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets([end], info.Start);
                            p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);
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
                        -"end" | (p => p.CurrentFunction.SetBreakTargets()),
                    ],
                    "While Case" %
                    [
                        "\"whilecase\" Keyword" % typeof(WhileCaseToken) | (p => p.CurrentFunction.IncreaseAllBreaks(1)),
                        -"$start" | (p => {
                            int start = p.CurrentFunction.CurrentPosition;
                            p.PushVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);
                                p.PushVariable(info);
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (p => {
                                int repeat = p.CurrentFunction.CurrentPosition;
                                p.AddInstruction(typeof(UnconditionalJumpInstruction), [], []);

                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);

                                int start = (int)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets([repeat], start);
                                p.PushVariable(start);
                            }),
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(CaseStartToken),
                        "Default Body" ^ statementMatcher,
                        -"end" | (p => p.CurrentFunction.SetBreakTargets()),
                    ],
                    "In Case" %
                    [
                        "\"incase\" Keyword" % typeof(InCaseToken) | (p => p.CurrentFunction.IncreaseAllBreaks(1)),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);
                                p.PushVariable(info);
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole incase
                            -"$false target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);
                            }),
                        ],
                        -"end" | (p => p.CurrentFunction.SetBreakTargets()),
                    ],
                    "Until Case" %
                    [
                        "\"untilcase\" Keyword" % typeof(UntilCaseToken) | (p => p.CurrentFunction.IncreaseAllBreaks(1)),
                        -"$start" | (p => {
                            int start = p.CurrentFunction.CurrentPosition;
                            p.PushVariable(start);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);
                                p.PushVariable(info);
                            }),
                            "When Body" ^ statementMatcher,
                            "Optional Semi Colon" ^ typeof(SemiColonToken),
                            -"$false target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);
                            }),
                        ],
                        "\"until\" Keyword" % typeof(UntilToken),
                        conditionMatcher,
                        -"$until" | (p => {
                            JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);

                            int start = (int)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets(info.FalseOriginList, start);

                            p.CurrentFunction.SetBreakTargets();
                        }),
                    ],
                    "For Case" %
                    [
                        "\"forcase\" Keyword" % typeof(ForCaseToken) | (p => p.CurrentFunction.IncreaseAllBreaks(2)),
                        "Iteration Identifier" % typeof(IdentifierToken),
                        -"initialize" | (p => {
                            // TODO: Add variable to symbol table
                            string iterationID = (string)p.PeekVariable();
                            p.AddInstruction(typeof(AssignmentInstruction), [typeof(string), typeof(object)], [iterationID, 0]);
                        }),
                        "Equals Sign" % OperatorToken.OperationType.EqualTo | (p => {
                            _ = p.PopVariable(); // Ignore Variable; it will always be an Equals Sign.
                        }),
                        expressionMatcher,
                        -"condition & increment" | (p =>
                        {
                            // Condition

                            ExpressionBlockInfo count = (ExpressionBlockInfo)p.PopVariable();
                            string iterationID = (string)p.PopVariable();
                            p.AddJumpInstructions(
                                typeof(ComparisonJumpInstruction),
                                [typeof(OperatorToken.OperationType), typeof(object), typeof(object)],
                                [OperatorToken.OperationType.LessThan, iterationID, count.Result],
                                count.Start);

                            JumpBlockInfo info = (JumpBlockInfo)p.PeekVariable();
                            p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);

                            // Increment

                            p.AddInstruction(
                                typeof(OperationInstruction),
                                [typeof(OperatorToken.OperationType), typeof(object), typeof(object), typeof(string)],
                                [OperatorToken.OperationType.Add, iterationID, 1, iterationID]);
                        }),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            -"$true target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.TrueOriginList, p.CurrentFunction.CurrentPosition);
                                p.PushVariable(info);
                            }),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole forcase
                            -"$false target" | (p => {
                                JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                                p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);
                            }),
                        ],
                        -"end" | (p => {
                            p.CurrentFunction.SetBreakTargets();

                            int repeat = p.CurrentFunction.CurrentPosition;
                            p.AddInstruction(typeof(UnconditionalJumpInstruction), [], []);

                            JumpBlockInfo info = (JumpBlockInfo)p.PopVariable();
                            p.CurrentFunction.SetJumpTargets(info.FalseOriginList, p.CurrentFunction.CurrentPosition);
                            p.CurrentFunction.SetJumpTargets([repeat], info.Start);

                            p.CurrentFunction.SetBreakTargets();
                        }),
                    ],
                    "Break" %
                    [
                        "\"break\" Keyword" % typeof(BreakToken),
                        "Break Count" % typeof(ConstantToken),
                    ] | (p => p.AddBreakInstruction((uint)p.PopVariable())),
                    "Return" %
                    [
                        "\"return\" Keyword" % typeof(ReturnToken),
                        expressionMatcher,
                    ] | (p => p.AddInstruction(typeof(ReturnInstruction), [typeof(object)], [((ExpressionBlockInfo)p.PopVariable()).Result])),
                    "Input" %
                    [
                        "\"input\" Keyword" % typeof(InputToken),
                        "Variable ID" % typeof(IdentifierToken),
                    ] | (p => p.AddInstruction(typeof(InputInstruction), [typeof(string)], [p.PopVariable()])),
                    "Print" %
                    [
                        "\"print\" Keyword" % typeof(PrintToken),
                        expressionMatcher,
                    ] | (p => p.AddInstruction(typeof(OutputInstruction), [typeof(object)], [((ExpressionBlockInfo)p.PopVariable()).Result])),
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
                        -"start" | (p => p.CurrentFunction.IncreaseAllBreaks(1)),
                        declarationsMatcher,
                        functionsMatcher,
                        statementsMatcher,
                        -"end"   | (p => p.CurrentFunction.SetBreakTargets()),
                    ]);

            superMatcher =
                "Program" %
                [
                    "\"program\" Keyword" % typeof(ProgramToken) | (p => p.CreateFunction()),
                    "Program ID" % typeof(IdentifierToken) | (p => p.CurrentFunction.Name = (string)p.PopVariable()),
                    "Program Body" % statementMatcher,
                    "EOF" % typeof(EOFToken),
                ] | (p => p.FinalizeFunction(typeof(HaltInstruction), [], []));
        }

        public void Analyse(BlockingCollection<Token> input, IntermediateProgram? output = null)
        {
            var tokens = input.GetConsumingEnumerable().GetEnumerator();
            TokenMatcher.MoveNext(tokens);
            if (superMatcher.TryMatch(tokens, output) == false)
                throw new SyntaxAnalyserException($"Expected {superMatcher.Name}: {tokens.Current}");
        }
    }
}