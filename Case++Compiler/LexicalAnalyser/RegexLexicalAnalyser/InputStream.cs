using System.Text;
using System.Text.RegularExpressions;

namespace CaseppCompiler.LexicalAnalyser.RegexLexicalAnalyser
{
    internal class InputStream(Stream byteStream, Encoding? encoding = null)
    {
        private const int MAX_REGEX = 30;

        private readonly StreamReader reader = new(byteStream, encoding);
        private char[] buffer = new char[2 * MAX_REGEX];
        private int start = 0, end = 0;

        public bool EndOfStream => start == end && reader.EndOfStream;

        public void Trim(ref int line, ref int column)
        {
            char current;
            CommentState commentState = CommentState.None;
            while (true)
            {
                if (start == end)
                {
                    if (reader.EndOfStream) return;
                    start = 0;
                    end = reader.ReadBlock(buffer, 0, 2 * MAX_REGEX);
                }
                current = buffer[start];
                switch (commentState)
                {
                    case CommentState.SingleLine:
                        if (current == '\n')
                        {
                            commentState = CommentState.None;
                            line++;
                            column = 0;
                        }
                        start++;
                        column++;
                        break;
                    case CommentState.MultiLine:
                        if (current == '*') commentState = CommentState.ExitingMultiLine;
                        else if (current == '\n') { line++; column = 0; }
                        start++;
                        column++;
                        break;
                    case CommentState.ExitingMultiLine:
                        switch (current)
                        {
                            case '\n':
                                line++;
                                column = 0;
                                goto default;
                            default:
                                commentState = CommentState.MultiLine;
                                break;
                            case '/':
                                commentState = CommentState.None;
                                break;
                            case '*':
                                break;
                        }
                        start++;
                        column++;
                        break;
                    default:
                        if (!char.IsWhiteSpace(current))
                        {
                            if (current != '/') return;
                            start++;
                            column++;
                            if (start == end)
                            {
                                if (reader.EndOfStream) { start--; return; }
                                buffer[0] = '/';
                                start = 1;
                                end = reader.ReadBlock(buffer, 1, 2 * MAX_REGEX - 1);
                            }
                            switch (buffer[start])
                            {
                                case '/':
                                    commentState = CommentState.SingleLine;
                                    start++;
                                    column++;
                                    continue;
                                case '*':
                                    commentState = CommentState.MultiLine;
                                    start++;
                                    column++;
                                    continue;
                                default:
                                    start--;
                                    column--;
                                    return;
                            }
                        }
                        if (current == '\n') { line++; column = 0; }
                        start++;
                        column++;
                        break;
                }
            }
        }

        private enum CommentState { None, SingleLine, MultiLine, ExitingMultiLine }

        public void Trim(Predicate<char> predicate, ref int line, ref int column)
        {
            char current;
            while (true)
            {
                if (start == end)
                {
                    if (reader.EndOfStream) return;
                    start = 0;
                    end = reader.ReadBlock(buffer, 0, 2 * MAX_REGEX);
                }
                current = buffer[start];
                if (!predicate(current)) return;
                if (current == '\n') { line++; column = 0; }
                start++;
                column++;
            }
        }

        public bool TryMatchFirst(IEnumerable<Regex> regexes, out string text)
        {
            int available = end - start;
            if (available < MAX_REGEX && !reader.EndOfStream)
            {
                Array.Copy(buffer, start, buffer, 0, available);
                start = 0;
                end = available + reader.ReadBlock(buffer, available, 2 * MAX_REGEX - available);
            }
            foreach (Regex regex in regexes)
            {
                var span = buffer.AsSpan()[start..end];
                var e = regex.EnumerateMatches(span);
                if (e.MoveNext())
                {
                    ValueMatch match = e.Current;
                    if (match.Index != 0) throw new ArgumentException("Token regex should only match if at the beginning.");
                    text = span[..match.Length].ToString();
                    start += match.Length;
                    return true;
                }
            }
            text = "";
            return false;
        }
    }
}
