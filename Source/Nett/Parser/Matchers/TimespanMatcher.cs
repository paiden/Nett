namespace Nett.Parser.Matchers
{
    using System;
    using System.Text;

    internal static class TimespanMatcher
    {
        internal static Token? ContinueMatchFromInteger(StringBuilder matchedAlready, CharBuffer cs)
        {
            var sb = matchedAlready;

            if (cs.TryExpect("."))
            {
                sb.Append(cs.Consume());
                sb.Append(cs.ExpectAndConsumeDigit());
                sb.Append(cs.ExpectAndConsumeDigit());
            }

            ExpectColonSperatedSegment(sb, cs);

            if (!cs.TokenDone())
            {
                ExpectColonSperatedSegment(sb, cs);
            }

            if (!cs.TokenDone() && cs.TryExpect('.'))
            {
                sb.Append(cs.Consume());
                while (!cs.End && cs.TryExpectDigit())
                {
                    sb.Append(cs.Consume());
                }
            }

            if (cs.TokenDone())
            {
                return new Token(TokenType.Timespan, sb.ToString());
            }
            else
            {
                throw Parser.CreateParseError(cs.FilePosition, $"Failed to parse timespan token because unexpected character '{cs.Peek().ToReadable()}' was found.");
            }
        }

        private static void ExpectColonSperatedSegment(StringBuilder sb, CharBuffer cs)
        {
            sb.Append(cs.ExpectAndConsume(':'));
            sb.Append(cs.ExpectAndConsumeDigit());
            sb.Append(cs.ExpectAndConsumeDigit());
        }
    }
}
