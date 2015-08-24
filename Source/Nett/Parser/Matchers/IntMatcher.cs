using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class IntMatcher : MatcherBase
    {
        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            if (cs.PeekIs('+') || cs.PeekIs('-') || cs.PeekInRange('0', '9'))
            {
                StringBuilder sb = new StringBuilder(16);
                sb.Append(cs.Consume());

                while (!cs.End && (cs.PeekInRange('0', '9') || cs.PeekIs('_')))
                {
                    sb.Append(cs.Consume());
                }

                if (cs.End || cs.PeekIsWhitespace())
                {
                    return new Token(TokenType.Integer, sb.ToString());
                }
                else
                {
                    if (cs.PeekIs('E') || cs.PeekIs('e') || cs.PeekIs('.'))
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
