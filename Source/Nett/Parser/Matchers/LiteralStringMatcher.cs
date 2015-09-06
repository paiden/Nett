using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class LiteralStringMatcher : MatcherBase
    {
        private const char StringTag = '\'';

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(256);
            if (!cs.Expect(StringTag))
            {
                return NoMatch;
            }

            sb.Append(cs.Consume());

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
                sb.Append(cs.Consume());
                return new Token(TokenType.LiteralString, sb.ToString());
            }
        }
    }
}
