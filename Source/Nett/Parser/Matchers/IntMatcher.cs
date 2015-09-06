using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class IntMatcher : MatcherBase
    {
        private bool hasPos = false;
        private bool hasSign = false;
        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            this.hasPos = cs.Expect('+');
            this.hasSign = this.hasPos || cs.Expect('-');

            if (this.hasSign || cs.ExpectInRange('0', '9'))
            {
                StringBuilder sb = new StringBuilder(16);
                sb.Append(cs.Consume());

                while (!cs.End && (cs.ExpectInRange('0', '9') || cs.Expect('_')))
                {
                    sb.Append(cs.Consume());
                }

                if (cs.Expect('-') && sb.Length == 4 && !this.hasSign)
                {
                    var dtm = new DateTimeMatcher(sb);
                    return dtm.Match(cs);
                }
                else if (cs.Expect('.') && cs.ExpectAt(3, ':') && !this.hasPos)
                {
                    var tsm = new TimespanMatcher(sb);
                    return tsm.Match(cs);
                }

                if (cs.End || cs.ExpectWhitespace())
                {
                    return new Token(TokenType.Integer, sb.ToString());
                }
                else
                {
                    if (cs.Expect('E') || cs.Expect('e') || cs.Expect('.'))
                    {
                        var matcher = new FloatMatcher(sb);
                        return matcher.Match(cs);
                    }
                    else
                    {
                        throw new NotSupportedException();
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
