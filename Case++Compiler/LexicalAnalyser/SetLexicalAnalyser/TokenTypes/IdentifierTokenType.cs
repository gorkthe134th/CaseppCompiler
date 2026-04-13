using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;

using System.Collections.Immutable;

namespace CaseppCompiler.LexicalAnalyser.SetLexicalAnalyser.TokenTypes
{
    internal class IdentifierTokenType : TokenType
    {
        private static readonly ImmutableDictionary<string, Func<int, int, Token>> keywordMap = new Dictionary<string, Func<int, int, Token>>()
        {
            [ "program"  ] = (line, column) => new ProgramToken(line, column),
            [ "declare"  ] = (line, column) => new DeclareToken(line, column),
            [    "if"    ] = (line, column) => new IfToken(line, column),
            [   "else"   ] = (line, column) => new ElseToken(line, column),
            [  "while"   ] = (line, column) => new WhileToken(line, column),
            ["switchcase"] = (line, column) => new SwitchCaseToken(line, column),
            [  "incase"  ] = (line, column) => new InCaseToken(line, column),
            ["whilecase" ] = (line, column) => new WhileCaseToken(line, column),
            ["untilcase" ] = (line, column) => new UntilCaseToken(line, column),
            [ "forcase"  ] = (line, column) => new ForCaseToken(line, column),
            [   "when"   ] = (line, column) => new WhenToken(line, column),
            [ "default"  ] = (line, column) => new DefaultToken(line, column),
            [  "until"   ] = (line, column) => new UntilToken(line, column),
            [  "break"   ] = (line, column) => new BreakToken(line, column),
            [  "repeat"  ] = (line, column) => new RepeatToken(line, column),
            [  "return"  ] = (line, column) => new ReturnToken(line, column),
            [  "print"   ] = (line, column) => new PrintToken(line, column),
            [  "input"   ] = (line, column) => new InputToken(line, column),
            [   "halt"   ] = (line, column) => new HaltToken(line, column),
            [   "jump"   ] = (line, column) => new JumpToken(line, column),
            [ "function" ] = (line, column) => new FunctionToken(line, column),
            [    "in"    ] = (line, column) => new InToken(line, column),
            [   "out"    ] = (line, column) => new OutToken(line, column),
            [  "inout"   ] = (line, column) => new InOutToken(line, column),
            [   "retv"   ] = (line, column) => new RetvToken(line, column),
            [   "par"    ] = (line, column) => new ParToken(line, column),
            [    "cv"    ] = (line, column) => new CVToken(line, column),
            [   "ref"    ] = (line, column) => new RefToken(line, column),
            [   "ret"    ] = (line, column) => new RetToken(line, column),
            [   "call"   ] = (line, column) => new CallToken(line, column),
            [   "true"   ] = (line, column) => new BoolConstantToken(true, line, column),
            [  "false"   ] = (line, column) => new BoolConstantToken(false, line, column),
        }.ToImmutableDictionary();

        private static readonly IImmutableSet<string> operatorOverrides = ["not", "and", "or"];

        public override IEnumerable<Func<char, bool?>> CharacterPredicates
        {
            get
            {
                yield return c => char.IsAsciiLetter(c) ? null : false;
                while (true)
                    yield return c => char.IsAsciiLetterOrDigit(c) ? null : false;
            }
        }

        public override int Limit => 30;

        public override Token GenerateToken(string text, int line, int column) =>
            keywordMap.TryGetValue(text, out var token) ? token(line, column) :
            operatorOverrides.Contains(text) ? new OperatorToken(text, line, column) :
            new IdentifierToken(text, line, column);
    }
}
