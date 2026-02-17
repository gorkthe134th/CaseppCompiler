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
            TokenMatcher declarationsMatcher = new KleeneStarTokenMatcher(
                "Variable Declarations",
                new SequenceTokenMatcher(
                    "Variable Declaration",
                    [
                        new TypeTokenMatcher<DeclareToken>("\"declare\" Keyword"),
                        new TypeTokenMatcher<IdentifierToken>("Variable ID"),
                        new TypeTokenMatcher<SemiColonToken>("Semi Colon"),
                    ]));

            superMatcher = new SequenceTokenMatcher(
                "Program",
                [
                    new TypeTokenMatcher<ProgramToken>("\"program\" Keyword"),
                    new TypeTokenMatcher<IdentifierToken>("Program ID"),
                    new BlockTokenMatcher("Program Body", declarationsMatcher),
                ]);
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
            if (tokens.MoveNext()) throw new ArgumentException($"Expected End Of File: {tokens.Current}");
        }
    }
}