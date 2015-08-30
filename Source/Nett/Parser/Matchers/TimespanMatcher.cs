using System.Diagnostics;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class TimespanMatcher : MatcherBase
    {
        private readonly StringBuilder sb;
        public TimespanMatcher(StringBuilder sb)
        {
            this.sb = sb;
        }

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            Debug.Assert(cs.PeekIs('.'));
            this.sb.Append(cs.Consume());

            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs('.')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }

            while (!cs.End && cs.PeekIsDigit())
            {
                this.sb.Append(cs.Consume());
            }

            if (cs.TokenDone())
            {
                return new Token(TokenType.Timespan, this.sb.ToString());
            }
            else
            {
                return NoMatch;
            }
        }
    }
}
