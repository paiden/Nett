namespace Nett.Parser.Matchers
{
    using System.Diagnostics;
    using System.Text;

    internal static class DateTimeMatcher
    {
        internal static Token? TryMatch(StringBuilder matchedAlready, CharBuffer cs)
        {
            var sb = matchedAlready;
            Debug.Assert(cs.TryExpect('-'));

            sb.Append(cs.Consume());

            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect('-')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect('T')) { sb.Append(cs.Consume()); } else { return FinishDatetimeToken(sb); }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
            if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
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
            else if (cs.TryExpect('-') || cs.TryExpect('+'))
            {
                sb.Append(cs.Consume());

                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }

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

                while (cs.TryExpectDigit())
                {
                    sb.Append(cs.Consume());
                }

                if (cs.TryExpect('-') || cs.TryExpect('+')) { sb.Append(cs.Consume()); } else { return FinishDatetimeToken(sb); }
                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpect(':')) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }
                if (cs.TryExpectDigit()) { sb.Append(cs.Consume()); } else { return null; }

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
                return FinishDatetimeToken(sb);
            }
        }

        private static Token? FinishDatetimeToken(StringBuilder sb)
        {
            return new Token(TokenType.DateTime, sb.ToString());
        }
    }
}
