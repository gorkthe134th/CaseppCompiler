using CaseppCompiler.LexicalAnalyser.Tokens;
using CaseppCompiler.LexicalAnalyser.Tokens.KeywordTokens;

using System.Text;
using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser.TokenTypes
{
    internal class KeywordTokenType : TokenType
    {
        private static readonly Dictionary<string, Func<int, int, Token>> tokenMap = new()
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
            [  "return"  ] = (line, column) => new ReturnToken(line, column),
            [  "print"   ] = (line, column) => new PrintToken(line, column),
            [  "input"   ] = (line, column) => new InputToken(line, column),
            [ "function" ] = (line, column) => new FunctionToken(line, column),
            [    "in"    ] = (line, column) => new InToken(line, column),
            [   "out"    ] = (line, column) => new OutToken(line, column),
            [  "inout"   ] = (line, column) => new InOutToken(line, column),
            [   "true"   ] = (line, column) => new BoolConstantToken(true, line, column),
            [  "false"   ] = (line, column) => new BoolConstantToken(false, line, column),
        };

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

        public override Token GenerateToken(string text, int line, int column) =>
            tokenMap[text](line, column);
    }
}
