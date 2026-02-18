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
                    "Semi Colon" % typeof(SemiColonToken),
                ];

            TokenMatcher formalParameterMatcher =
                "Formal Parameter" |
                [
                    "In Parameter" %
                    [
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

            TokenMatcher actualParameterMatcher =
                "Actual Parameter" |
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
                "Single Statement of Block Body" |
                [
                    "Single Statement" %
                    [
                        singleStatementMatcher,
                        "Optional Semi Colon" ^
                            "Semi Colon" % typeof(SemiColonToken),
                    ],
                    blockBodyMatcher,
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
                        ],
                        "\"default\" Keyword" % typeof(DefaultToken),
                        "Colon" % typeof(CaseStartToken),
                        controlBody,
                    ],
                ]);

            UnresolvedTokenMatcher statementsMatcher = new("Statements");
            statementsMatcher.Resolve(
                "Statements" ^
                [
                    singleStatementMatcher,
                    "Continuation" ^
                    [
                        "Semi Colon" % typeof(SemiColonToken),
                        statementsMatcher,
                    ],
                ]);

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
            if (!tokens.MoveNext())
            {
                if(!superMatcher.CanMatchEmpty) throw new ArgumentException($"Expected {superMatcher.Name}");
                return;
            }
            if (!superMatcher.CanMatch(tokens.Current)) throw new ArgumentException($"Expected {superMatcher.Name}: {tokens.Current}");
            superMatcher.Match(tokens);
        }
    }
}