using System;
using System.Linq;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class MultilineLiteralStringMatcher : MatcherBase
    {
        private const string StringTag = "'''";

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(256);
            if (!cs.Expect(StringTag))
            {
                return NoMatch;
            }

            sb.Append(cs.Consume(3).ToArray());

            while (!cs.End && !cs.Expect(StringTag))
            {
                sb.Append(cs.Consume());
            }

            if (!cs.Expect(StringTag))
            {
                throw new Exception($"Closing '{StringTag}' not found for literal string '{sb.ToString()}'");
            }
            else
            {
                sb.Append(cs.Consume(StringTag.Length).ToArray());
                return new Token(TokenType.MultilineLiteralString, sb.ToString());
            }
        }
    }
}
