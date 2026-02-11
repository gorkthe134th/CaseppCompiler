using CaseppCompiler.Tokens;

using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CaseppCompiler
{
    internal class InputStream(Stream byteStream, Encoding? encoding = null)
    {
        private const int MAX_REGEX = 30;

        StreamReader reader = new(byteStream, encoding);
        char[] buffer = new char[2 * MAX_REGEX];
        int start = 0, end = 0;

        public bool EndOfStream => start == end && reader.EndOfStream;

        public void Trim(out int lineChanges)
        {
            lineChanges = 0;
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
                if (!char.IsWhiteSpace(current)) break;
                if (current == '\n') lineChanges++;
                start++;
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
