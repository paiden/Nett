using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class FloatMatcher : MatcherBase
    {
        private readonly StringBuilder sb;
        public FloatMatcher(StringBuilder beforeFraction)
        {
            this.sb = beforeFraction;
        }

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            if (cs.TryExpect('.'))
            {
                this.sb.Append(cs.Consume());
                var im = new IntMatcher();
                var intFrac = im.Match(cs);
                this.sb.Append(intFrac.Value.value);
            }

            if (cs.TryExpect('e') || cs.TryExpect('E'))
            {
                this.sb.Append(cs.Consume());
                var fractionMatcher = new IntMatcher();
                var intPar = fractionMatcher.Match(cs);
                this.sb.Append(intPar.Value.value);
            }

            if (cs.TokenDone())
            {
                return new Token(TokenType.Float, this.sb.ToString());
            }
            else
            {
                throw new Exception("Failed to construct float token"); //TODO better error message.
            }
        }
    }
}
