using System.Diagnostics;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal static class TimespanMatcher
    {
        internal static Token? TryMatch(StringBuilder matchedAlready, LookaheadBuffer<char> cs)
        {
            var sb = matchedAlready;
            Debug.Assert(cs.TryExpect('.'));
            sb.Append(cs.Consume());

            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect('.')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }

            while (!cs.End && cs.ExpectDigit())
            {
                sb.Append(cs.Consume());
            }

            if (cs.TokenDone())
            {
                return new Token(TokenType.Timespan, sb.ToString());
            }
            else
            {
                return null;
            }
        }
    }
}
