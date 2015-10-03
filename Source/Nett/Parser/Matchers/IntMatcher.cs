using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class IntMatcher : MatcherBase
    {
        private bool hasPos = false;
        private bool hasSign = false;
        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            this.hasPos = cs.TryExpect('+');
            this.hasSign = this.hasPos || cs.TryExpect('-');

            if (this.hasSign || cs.ExpectInRange('0', '9'))
            {
                StringBuilder sb = new StringBuilder(16);
                sb.Append(cs.Consume());

                while (!cs.End && (cs.ExpectInRange('0', '9') || cs.TryExpect('_')))
                {
                    sb.Append(cs.Consume());
                }

                if (cs.TryExpect('-') && sb.Length == 4 && !this.hasSign)
                {
                    var dtm = new DateTimeMatcher(sb);
                    return dtm.Match(cs);
                }
                else if (cs.TryExpect('.') && cs.ExpectAt(3, ':') && !this.hasPos)
                {
                    var tsm = new TimespanMatcher(sb);
                    return tsm.Match(cs);
                }

                if (cs.End || cs.ExpectWhitespace() || cs.TokenDone())
                {
                    return new Token(TokenType.Integer, sb.ToString());
                }
                else
                {
                    if (cs.TryExpect('E') || cs.TryExpect('e') || cs.TryExpect('.'))
                    {
                        var matcher = new FloatMatcher(sb);
                        return matcher.Match(cs);
                    }
                    else
                    {
                        var matcher = new BareKeyMatcher(sb);
                        return matcher.Match(cs);
                    }
                }
            }
            else
            {
                return new Token?();
            }
        }
    }
}
