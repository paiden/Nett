using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class StringMatcher : MatcherBase
    {
        private const char StringTag = '\"';

        private readonly MultilineStringMatcher multilineStringMatcher = new MultilineStringMatcher();

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(128);
            if (!cs.TryExpect(StringTag))
            {
                return NoMatch;
            }

            if (cs.ItemsAvailable > 2 && cs.ExpectAt(1, StringTag) && cs.ExpectAt(2, StringTag))
            {
                return this.multilineStringMatcher.Match(cs);
            }

            sb.Append(cs.Consume());

            while (cs.Peek() != StringTag)
            {
                if (cs.TryExpect('\\'))
                {
                    sb.Append(cs.Consume());
                }

                sb.Append(cs.Consume());
            }

            if (!cs.TryExpect(StringTag))
            {
                throw new Exception($"Closing '{StringTag}' not found for string {sb.ToString()}'");
            }
            else
            {
                sb.Append(cs.Consume());
                return new Token(TokenType.String, sb.ToString());
            }
        }
    }
}
