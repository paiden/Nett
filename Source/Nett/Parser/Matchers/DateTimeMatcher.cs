using System.Diagnostics;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class DateTimeMatcher : MatcherBase
    {
        private readonly StringBuilder sb;
        public DateTimeMatcher(StringBuilder sb)
        {
            this.sb = sb;
        }

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            Debug.Assert(cs.PeekIs('-'));

            this.sb.Append(cs.Consume());

            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs('-')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs('T')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.PeekIs('Z'))
            {
                this.sb.Append(cs.Consume());
                if (cs.TokenDone())
                {
                    return this.FinishDatetimeToken();
                }
                else
                {
                    return NoMatch;
                }
            }
            else if (cs.PeekIs('-'))
            {
                this.sb.Append(cs.Consume());

                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIs(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }

                if (cs.TokenDone())
                {
                    return this.FinishDatetimeToken();
                }
                else
                {
                    return NoMatch;
                }
            }
            else if (cs.PeekIs('.'))
            {
                this.sb.Append(cs.Consume());

                while (cs.PeekIsDigit())
                {
                    this.sb.Append(cs.Consume());
                }

                if (cs.PeekIs('-')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIs(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.PeekIsDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }

                if (cs.TokenDone())
                {
                    return this.FinishDatetimeToken();
                }
                else
                {
                    return NoMatch;
                }
            }
            else
            {
                return NoMatch;
            }
        }

        private Token? FinishDatetimeToken()
        {
            return new Token(TokenType.DateTime, this.sb.ToString());
        }
    }
}
