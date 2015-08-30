using System.Collections.Generic;
using System.Linq;

namespace Nett.Parser.Matchers
{
    internal static class BufferExtensions
    {
        private static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        public static bool PeekInRange(this LookaheadBuffer<char> buffer, char min, char max)
        {
            char pv = buffer.Peek();
            return pv >= min && pv <= max;
        }

        public static bool PeekIsWhitespace(this LookaheadBuffer<char> buffer)
        {
            char pv = buffer.Peek();
            return WhitspaceCharSet.Contains(pv);
        }

        public static bool TokenDone(this LookaheadBuffer<char> buffer)
        {
            return buffer.End || buffer.PeekIsWhitespace();
        }

        public static bool LaSequenceIs(this LookaheadBuffer<char> buffer, string seq)
        {
            for (int i = 0; i < seq.Length; i++)
            {
                if (buffer.La(i) != seq[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<char> Consume(this LookaheadBuffer<char> buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return buffer.Consume();
            }
        }

        public static bool PeekIsDigit(this LookaheadBuffer<char> buffer)
        {
            return buffer.PeekInRange('0', '9');
        }
    }
}
