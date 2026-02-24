using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;
using CaseppCompiler.SyntaxAnalyser.GrammarSyntaxAnalyser.TokenMatchers;

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
                            "Variable IDs" %
                            [
                                "Variable ID" % typeof(IdentifierToken),
                                "More Variables" *
                                [
                                    "Comma" % typeof(CommaToken),
                                    "Variable ID" % typeof(IdentifierToken),
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
                    ],
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ],
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ],
                ];

            UnresolvedTokenMatcher blockBodyMatcher = new("Block Body");

            TokenMatcher functionsMatcher =
                "Functions" * 
                [
                    "\"function\" Keyword" % typeof(FunctionToken),
                    "Function ID" % typeof(IdentifierToken),
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
                    blockBodyMatcher,
                ];

            UnresolvedTokenMatcher expressionMatcher = new("Expression");

            TokenMatcher actualParameterMatcher =
                "Actual Parameter" |
                [
                    "In Parameter" %
                    [
                        "Optional Keyword" ^
                            "\"in\" Keyword" % typeof(InToken),
                        expressionMatcher,
                    ],
                    "Out Parameter" %
                    [
                        "\"out\" Keyword" % typeof(OutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ],
                    "InOut Parameter" %
                    [
                        "\"inout\" Keyword" % typeof(InOutToken),
                        "Parameter ID" % typeof(IdentifierToken),
                    ],
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
                    "More Factors" *
                    [
                        "Multiplicative Operation" |
                        [
                            "Multiply" % OperatorToken.OperationType.Multiply,
                            "Divide" % OperatorToken.OperationType.Divide,
                        ],
                        factorMatcher,
                    ],
                ];

            expressionMatcher.Resolve(
                "Expression" %
                [
                    "Optional Sign" ^ (
                        "Sign" |
                        [
                            "Plus" % OperatorToken.OperationType.Add,
                            "Minus" % OperatorToken.OperationType.Subtract,
                        ]),
                    termMatcher,
                    "More Terms" *
                    [
                        "Additive Operation" |
                        [
                            "Add" % OperatorToken.OperationType.Add,
                            "Subtract" % OperatorToken.OperationType.Subtract,
                        ],
                        termMatcher,
                    ],
                ]);

            UnresolvedTokenMatcher conditionMatcher = new("Condition");

            TokenMatcher boolFactorMatcher =
                "Bool Factor" |
                [
                    "Constant" % typeof(BoolConstantToken),
                    "Inverted Sub-Condition" %
                    [
                        "\"not\" Keyword" % OperatorToken.OperationType.Not,
                        "Sub-Condition" >= conditionMatcher,
                    ],
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
                    ],
                ];

            TokenMatcher boolTermMatcher =
                "Bool Term" %
                [
                    boolFactorMatcher,
                    "More Bool Factors" *
                    [
                        "And" % OperatorToken.OperationType.And,
                        boolFactorMatcher,
                    ],
                ];

            conditionMatcher.Resolve(
                "Condition" %
                [
                    boolTermMatcher,
                    "More Bool Terms" *
                    [
                        "Or" % OperatorToken.OperationType.Or,
                        boolTermMatcher,
                    ],
                ]);

            UnresolvedTokenMatcher singleStatementMatcher = new("Single Statement");

            TokenMatcher controlBody =
                "Control Body" |
                [
                    blockBodyMatcher,
                    "Single Statement" ^ singleStatementMatcher,
                    // Cannot allow a semi colon here because it's ambiguous
                    // whether a semi colon ends the control body or the whole statement (if, while, etc.)
                ];

            singleStatementMatcher.Resolve(
                "Statement" |
                [
                    "Assignment" %
                    [
                        "Variable ID" % typeof(IdentifierToken),
                        "Assignment Token" % typeof(AssignmentToken),
                        expressionMatcher,
                    ],
                    "If" %
                    [
                        "\"if\" Keyword" % typeof(IfToken),
                        conditionMatcher,
                        controlBody,
                        "Optional Else" ^
                            "Else" %
                            [
                                //"Optional Semi Colon" ^
                                //    "Semi Colon" % typeof(SemiColonToken),
                                // Cannot allow a semi colon here because it's ambiguous
                                // whether a semi colon ends the main body or the whole if
                                "\"else\" Keyword" % typeof(ElseToken),
                                controlBody,
                            ],
                    ],
                    "While" %
                    [
                        "\"while\" Keyword" % typeof(WhileToken),
                        conditionMatcher,
                        controlBody,
                        "Optional Else" ^
                            "Else" %
                            [
                                //"Optional Semi Colon" ^
                                //    "Semi Colon" % typeof(SemiColonToken),
                                // Cannot allow a semi colon here because it's ambiguous
                                // whether a semi colon ends the main body or the whole while
                                "\"else\" Keyword" % typeof(ElseToken),
                                controlBody,
                            ],
                    ],
                    "Switch Case" %
                    [
                        "\"switchcase\" Keyword" % typeof(SwitchCaseToken),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            controlBody,
                            "Optional Semi Colon" ^
                                "Semi Colon" % typeof(SemiColonToken),
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(CaseStartToken),
                        controlBody,
                    ],
                    "While Case" %
                    [
                        "\"whilecase\" Keyword" % typeof(WhileCaseToken),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            controlBody,
                            "Optional Semi Colon" ^
                                "Semi Colon" % typeof(SemiColonToken),
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(CaseStartToken),
                        controlBody,
                    ],
                    "In Case" %
                    [
                        "\"incase\" Keyword" % typeof(InCaseToken),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            controlBody,
                            //"Optional Semi Colon" ^
                            //    "Semi Colon" % typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole incase
                        ],
                    ],
                    "For Case" %
                    [
                        "\"forcase\" Keyword" % typeof(ForCaseToken),
                        "Iteration Identifier" % typeof(IdentifierToken),
                        "Equals Sign" % OperatorToken.OperationType.EqualTo,
                        expressionMatcher,
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            controlBody,
                            //"Optional Semi Colon" ^
                            //    "Semi Colon" % typeof(SemiColonToken),
                            // Cannot allow a semi colon here because it's ambiguous
                            // whether a semi colon ends the current case or the whole forcase
                        ],
                    ],
                    "Until Case" %
                    [
                        "\"untilcase\" Keyword" % typeof(UntilCaseToken),
                        "Cases" *
                        [
                            "\"when\" Keyword" % typeof(WhenToken),
                            conditionMatcher,
                            "Colon" % typeof(CaseStartToken),
                            controlBody,
                            "Optional Semi Colon" ^
                                "Semi Colon" % typeof(SemiColonToken),
                        ],
                        "\"until\" Keyword" % typeof(UntilToken),
                        conditionMatcher,
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
                    "Statement" ^ singleStatementMatcher,
                    "Continuation" *
                    [
                        "Semi Colon" % typeof(SemiColonToken),
                        "Statement" ^ singleStatementMatcher,
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
                    "Program ID" % typeof(IdentifierToken),
                    blockBodyMatcher,
                    "EOF" % typeof(EOFToken),
                ];
        }

        public void Analyse(IEnumerable<Token> input)
        {
            var tokens = input.GetEnumerator();
            TokenMatcher.MoveNext(tokens);
            if (superMatcher.TryMatch(tokens) == false)
                throw new SyntaxAnalyserException($"Expected {superMatcher.Name}: {tokens.Current}");
        }
    }
}