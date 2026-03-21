using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage;
using CaseppCompiler.SyntaxAnalyser.IntermediateLanguage.IntermediateInstructions;

using System.Diagnostics;

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
                    ] | (p => p.FinalizeFunction())
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

            TokenMatcher factorMatcher =
                "Factor" |
                [
                    "Constant" % typeof(ConstantToken),
                    "Sub-Expression" > expressionMatcher,
                    "Identifier" %
                    [
                        "Name" % typeof(IdentifierToken),
                        "Optional Parameters" ^
                            "Actual Parameter List" > (
                                "Actual Parameters" ^
                                [
                                    actualParameterMatcher,
                                    "More Parameters" *
                                    [
                                        "Comma" % typeof(CommaToken),
                                        actualParameterMatcher,
                                    ],
                                ]),
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
                            object operand2ID = p.PopVariable();
                            OperatorToken.OperationType operation = (OperatorToken.OperationType)p.PopVariable();
                            object operand1ID = p.PopVariable();
                            string tempID = p.GenerateTemp();
                            p.AddInstruction(
                                typeof(OperationInstruction),
                                [typeof(OperatorToken.OperationType), typeof(object), typeof(object), typeof(string)],
                                [operation, operand1ID, operand2ID, tempID]);
                            p.PushVariable(tempID);
                        })),
                ];

            expressionMatcher.Resolve(
                "Expression" %
                [
                    "Optional Sign" ^ (
                        "Sign" |
                        [
                            "Plus" % OperatorToken.OperationType.Add,
                            "Minus" % OperatorToken.OperationType.Subtract,
                        ] | (p => _ = p.PopVariable() /* Temporarily Ignore */ )),
                    termMatcher,
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
                            object operand2ID = p.PopVariable();
                            OperatorToken.OperationType operation = (OperatorToken.OperationType)p.PopVariable();
                            object operand1ID = p.PopVariable();
                            string tempID = p.GenerateTemp();
                            p.AddInstruction(
                                typeof(OperationInstruction),
                                [typeof(OperatorToken.OperationType), typeof(object), typeof(object), typeof(string)],
                                [operation, operand1ID, operand2ID, tempID]);
                            p.PushVariable(tempID);
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
                        _ = p.PopVariable();
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
                        object operand2ID = p.PopVariable();
                        OperatorToken.OperationType operation = (OperatorToken.OperationType)p.PopVariable();
                        object operand1ID = p.PopVariable();
                        p.AddJumpInstructions(
                            typeof(ComparisonJumpInstruction),
                            [typeof(OperatorToken.OperationType), typeof(object), typeof(object)],
                            [operation, operand1ID, operand2ID]);
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
                            _ = p.PopVariable();
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
                            _ = p.PopVariable();
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
                        object resultID = p.PopVariable();
                        string variableID = (string)p.PopVariable();
                        p.AddInstruction(
                            typeof(AssignmentInstruction),
                            [typeof(string), typeof(object)],
                            [variableID, resultID]);
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
                    "While" %
                    [
                        "\"while\" Keyword" % typeof(WhileToken),
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
                    "While Case" %
                    [
                        "\"whilecase\" Keyword" % typeof(WhileCaseToken),
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
                    ],
                    "In Case" %
                    [
                        "\"incase\" Keyword" % typeof(InCaseToken),
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
                    ],
                    "For Case" %
                    [
                        "\"forcase\" Keyword" % typeof(ForCaseToken),
                        "Iteration Identifier" % typeof(IdentifierToken) | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
                        "Equals Sign" % OperatorToken.OperationType.EqualTo | (p => {
                            _ = p.PopVariable(); // Ignore Variable; it will always be an Equals Sign.
                        }),
                        expressionMatcher,
                        -"" | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            -"" | (p => _ = p.PopVariable() /* Temporarily Ignore */ ),
                            "Colon" % typeof(CaseStartToken),
                            "When Body" ^ statementMatcher,
                            // "Optional Semi Colon" ^ typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole forcase
                        ],
                    ],
                    "Until Case" %
                    [
                        "\"untilcase\" Keyword" % typeof(UntilCaseToken),
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
                        }),
                    ],
                    "Return" %
                    [
                        "\"return\" Keyword" % typeof(ReturnToken),
                        expressionMatcher,
                    ],
                    "Input" %
                    [
                        "\"input\" Keyword" % typeof(InputToken),
                        "Variable ID" % typeof(IdentifierToken),
                    ],
                    "Print" %
                    [
                        "\"print\" Keyword" % typeof(PrintToken),
                        expressionMatcher,
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
                        declarationsMatcher,
                        functionsMatcher,
                        statementsMatcher,
                    ]);

            superMatcher =
                "Program" %
                [
                    "\"program\" Keyword" % typeof(ProgramToken),
                    "Program ID" % typeof(IdentifierToken) | (p => p.Main.Name = (string)p.PopVariable()),
                    "Program Body" % blockBodyMatcher,
                    "EOF" % typeof(EOFToken),
                ] | (p => p.FinalizeFunction());
        }

        public IntermediateProgram Analyse(IEnumerable<Token> input)
        {
            IntermediateProgram program = new();
            var tokens = input.GetEnumerator();
            TokenMatcher.MoveNext(tokens);
            if (superMatcher.TryMatch(tokens, program) == false)
                throw new SyntaxAnalyserException($"Expected {superMatcher.Name}: {tokens.Current}");
            return program;
        }
    }
}