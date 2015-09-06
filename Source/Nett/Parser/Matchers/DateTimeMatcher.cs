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
            Debug.Assert(cs.Expect('-'));

            this.sb.Append(cs.Consume());

            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.Expect('-')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.Expect('T')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.Expect(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.Expect(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
            if (cs.Expect('Z'))
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
            else if (cs.Expect('-'))
            {
                this.sb.Append(cs.Consume());

                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.Expect(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }

                if (cs.TokenDone())
                {
                    return this.FinishDatetimeToken();
                }
                else
                {
                    return NoMatch;
                }
            }
            else if (cs.Expect('.'))
            {
                this.sb.Append(cs.Consume());

                while (cs.ExpectDigit())
                {
                    this.sb.Append(cs.Consume());
                }

                if (cs.Expect('-')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.Expect(':')) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }
                if (cs.ExpectDigit()) { this.sb.Append(cs.Consume()); } else { return NoMatch; }

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
