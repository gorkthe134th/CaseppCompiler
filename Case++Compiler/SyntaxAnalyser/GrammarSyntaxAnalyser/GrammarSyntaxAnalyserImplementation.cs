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
                "Variable Declarations" * (
                    "Variable Declaration" % [
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
                    ]);

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

            UnresolvedTokenMatcher functionBody = new("Function Body");
            TokenMatcher functionsMatcher =
                "Functions" * (
                    "Function" % [
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
                        functionBody,
                    ]);

            TokenMatcher singleStatementMatcher =
                "Statement" |
                [
                    // TODO : Create Statement Matchers
                ];

            TokenMatcher statementsMatcher =
                "Statement Sequence" ^
                    "Statements" %
                    [
                        singleStatementMatcher,
                        "More Statements" *
                        [
                            "Semi Colon" % typeof(SemiColonToken),
                            singleStatementMatcher,
                        ],
                        "Optional Semi Colon" ^
                            "Semi Colon" % typeof(SemiColonToken),
                    ];

            functionBody.Resolve(
                "Function Body" >>
                    "Function Body Contents" %
                    [
                        declarationsMatcher,
                        functionsMatcher,
                    ]);

            superMatcher =
                "Program" %
                [
                    "\"program\" Keyword" % typeof(ProgramToken),
                    "Program ID" % typeof(IdentifierToken),
                    "Program Body" >>
                        "Program Body Contents" %
                        [
                            declarationsMatcher,
                            functionsMatcher,
                        ],
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