using System.Text;

namespace Nett.Parser.Matchers
{
    internal static class IntMatcher
    {
        internal static Token? TryMatch(LookaheadBuffer<char> cs)
        {
            bool hasPos = cs.TryExpect('+');
            bool hasSign = hasPos || cs.TryExpect('-');

            if (hasSign || cs.ExpectInRange('0', '9'))
            {
                StringBuilder sb = new StringBuilder(16);
                sb.Append(cs.Consume());

                while (!cs.End && (cs.ExpectInRange('0', '9') || cs.TryExpect('_')))
                {
                    sb.Append(cs.Consume());
                }

                if (cs.TryExpect('-') && sb.Length == 4 && !hasSign)
                {
                    return DateTimeMatcher.TryMatch(sb, cs);
                }
                else if (cs.TryExpect('.') && cs.ExpectAt(3, ':') && !hasPos)
                {
                    return TimespanMatcher.TryMatch(sb, cs);
                }

                if (cs.End || cs.ExpectWhitespace() || cs.TokenDone())
                {
                    return new Token(TokenType.Integer, sb.ToString());
                }
                else
                {
                    if (cs.TryExpect('E') || cs.TryExpect('e') || cs.TryExpect('.'))
                    {
                        return FloatMatcher.TryMatch(sb, cs);
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
