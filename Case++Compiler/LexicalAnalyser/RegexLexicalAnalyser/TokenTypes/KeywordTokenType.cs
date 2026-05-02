using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;

using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal class KeywordTokenType : TokenType
    {
        private static readonly ImmutableDictionary<string, Func<Position, Token>> tokenMap = new Dictionary<string, Func<Position, Token>>()
        {
            [ "program"  ] = (position) => new ProgramToken(position),
            [ "declare"  ] = (position) => new DeclareToken(position),
            [    "if"    ] = (position) => new IfToken(position),
            [   "else"   ] = (position) => new ElseToken(position),
            [  "while"   ] = (position) => new WhileToken(position),
            ["switchcase"] = (position) => new SwitchCaseToken(position),
            [  "incase"  ] = (position) => new InCaseToken(position),
            ["whilecase" ] = (position) => new WhileCaseToken(position),
            ["untilcase" ] = (position) => new UntilCaseToken(position),
            [ "forcase"  ] = (position) => new ForCaseToken(position),
            [   "when"   ] = (position) => new WhenToken(position),
            [ "default"  ] = (position) => new DefaultToken(position),
            [  "until"   ] = (position) => new UntilToken(position),
            [  "break"   ] = (position) => new BreakToken(position),
            [  "repeat"  ] = (position) => new RepeatToken(position),
            [  "return"  ] = (position) => new ReturnToken(position),
            [  "print"   ] = (position) => new PrintToken(position),
            [  "input"   ] = (position) => new InputToken(position),
            [   "halt"   ] = (position) => new HaltToken(position),
            [   "jump"   ] = (position) => new JumpToken(position),
            [ "function" ] = (position) => new FunctionToken(position),
            [    "in"    ] = (position) => new InToken(position),
            [   "out"    ] = (position) => new OutToken(position),
            [  "inout"   ] = (position) => new InOutToken(position),
            [   "retv"   ] = (position) => new RetvToken(position),
            [   "par"    ] = (position) => new ParToken(position),
            [    "cv"    ] = (position) => new CVToken(position),
            [   "ref"    ] = (position) => new RefToken(position),
            [   "ret"    ] = (position) => new RetToken(position),
            [   "call"   ] = (position) => new CallToken(position),
            [   "true"   ] = (position) => new BoolConstantToken(position, true),
            [  "false"   ] = (position) => new BoolConstantToken(position, false),
        }.ToImmutableDictionary();

        private static readonly Regex regex;
        public override Regex Regex => regex;

        static KeywordTokenType()
        {
            var keys = tokenMap.Keys.OrderByDescending(k => k.Length);
            StringBuilder builder = new(keys.Sum(k => k.Length + 3) + 2);
            builder.Append("^((");
            builder.AppendJoin(")|(", keys);
            builder.Append("))");
            regex = new(builder.ToString());
        }

        public override Predicate<char>? Trim => null;

        public override Token GenerateToken(Position position, string text) =>
            tokenMap[text](position);
    }
}
