using System.Diagnostics;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal static class DateTimeMatcher
    {
        internal static Token? TryMatch(StringBuilder matchedAlready, LookaheadBuffer<char> cs)
        {
            var sb = matchedAlready;
            Debug.Assert(cs.TryExpect('-'));

            sb.Append(cs.Consume());

            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect('-')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect('T')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect('Z'))
            {
                sb.Append(cs.Consume());
                if (cs.TokenDone())
                {
                    return FinishDatetimeToken(sb);
                }
                else
                {
                    return null;
                }
            }
            else if (cs.TryExpect('-'))
            {
                sb.Append(cs.Consume());

                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }

                if (cs.TokenDone())
                {
                    return FinishDatetimeToken(sb);
                }
                else
                {
                    return null;
                }
            }
            else if (cs.TryExpect('.'))
            {
                sb.Append(cs.Consume());

                while (cs.ExpectDigit())
                {
                    sb.Append(cs.Consume());
                }

                if (cs.TryExpect('-')) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.ExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }

                if (cs.TokenDone())
                {
                    return FinishDatetimeToken(sb);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static Token? FinishDatetimeToken(StringBuilder sb)
        {
            return new Token(TokenType.DateTime, sb.ToString());
        }
    }
}
