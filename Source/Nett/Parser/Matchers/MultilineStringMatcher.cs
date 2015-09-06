using System;
using System.Linq;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class MultilineStringMatcher : MatcherBase
    {
        private const string StringTag = "\"\"\"";

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(128);
            if (!cs.Expect(StringTag))
            {
                return NoMatch;
            }

            sb.Append(cs.Consume(StringTag.Length).ToArray());

            while (!cs.Expect(StringTag))
            {
                if (cs.End)
                {
                    break;
                }

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
                sb.Append(cs.Consume(3).ToArray());
                return new Token(TokenType.MultilineString, sb.ToString());
            }
        }
    }
}
