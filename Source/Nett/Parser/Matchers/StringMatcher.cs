using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class StringMatcher : MatcherBase
    {
        private const char StringTag = '\"';

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(128);
            if (!cs.Expect(StringTag))
            {
                return NoMatch;
            }

            sb.Append(cs.Consume());

            while (cs.Peek() != StringTag)
            {
                if (cs.Expect('\\'))
                {
                    sb.Append(cs.Consume());
                }

                sb.Append(cs.Consume());
            }

            if (!cs.Expect(StringTag))
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
