namespace Nett.Parser.Matchers
{
    using System.Text;

    internal static class IntMatcher
    {
        private const int BufferSize = 32; // 2^64 = +18_446_744_073_709_551_616 < 32 chars

        internal static Token? TryMatch(CharBuffer cs)
        {
            bool hasPos = cs.TryExpect('+');
            bool hasSign = hasPos || cs.TryExpect('-');

            if (hasSign || cs.TryExpectInRange('0', '9'))
            {
                StringBuilder sb = new StringBuilder(BufferSize);
                sb.Append(cs.Consume());

                if (hasSign && !cs.TryExpectInRange('0', '9'))
                {
                    throw Parser.CreateParseError(
                        cs.FilePosition, $"Failed to read Integer. Expected a number but '{cs.Peek().ToReadable()}' was found instead.");
                }

                while (!cs.End && (cs.TryExpectInRange('0', '9') || cs.TryExpect('_')))
                {
                    sb.Append(cs.Consume());
                }

                if (cs.TryExpect('-') && sb.Length == 4 && !hasSign)
                {
                    return DateTimeMatcher.TryMatch(sb, cs);
                }
                else if (((cs.TryExpect('.') && cs.TryExpectAt(3, ':')) || cs.TryExpect(":")) && !hasPos)
                {
                    return TimespanMatcher.ContinueMatchFromInteger(sb, cs);
                }

                if (cs.End || cs.TryExpectWhitespace() || cs.TokenDone())
                {
                    return new Token(TokenType.Integer, sb.ToString());
                }
                else
                {
                    if (cs.TryExpect('E') || cs.TryExpect('e') || cs.TryExpect('.'))
                    {
                        return FloatMatcher.Match(sb, cs);
                    }
                    else
                    {
                        return BareKeyMatcher.TryContinueMatch(sb, cs);
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}
