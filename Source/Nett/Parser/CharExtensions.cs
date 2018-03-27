using System.Linq;

namespace Nett.Parser
{
    internal static class CharExtensions
    {
        internal static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        public static bool InRange(this char c, char min, char max)
        {
            return min <= c && c <= max;
        }

        public static bool IsBareKeyChar(this char c)
        {
            return c.InRange('a', 'z') || c.InRange('A', 'Z') || c.InRange('0', '9') || c == '-' || c == '_';
        }

        public static bool Is(this char c, char c1, char c2)
            => c == c1 || c == c2;

        public static bool Is(this char c, char c1, char c2, char c3)
            => c == c1 || c == c2 || c == c3;

        public static string ToReadable(this char c)
        {
            if (c == '\0') { return "<EndOfFile>"; }

            return new string(c, 1);
        }

        public static bool IsDigit(this char c)
            => c.InRange('0', '9');

        public static bool IsNonZeroDigit(this char c)
            => c.InRange('1', '9');

        public static bool IsHexChar(this char c)
            => c.InRange('0', '9') || c.InRange('a', 'f') || c.InRange('A', 'F');

        public static bool IsOctalChar(this char c)
            => c.InRange('0', '7');

        public static bool IsBinChar(this char c)
            => c == '0' || c == '1';

        public static bool IsNumChar(this char c)
            => c.IsDigit() || c == '_';

        public static bool IsExponent(this char c)
            => c == 'e' || c == 'E';

        public static bool IsTokenSepChar(this char c)
            => c == ' ' || c == '\t' || c == '=' || c == ']' || c == ','
                || c == '}' || c == '\r' || c == '\n' || c == '#' || c == LexInput.EofChar;

        public static bool IsWhitespaceChar(this char c)
            => WhitspaceCharSet.Contains(c);
    }
}
