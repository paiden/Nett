using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal static class StringMatcher
    {
        private const char StringTag = '\"';

        internal static Token? TryMatch(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(128);
            if (!cs.TryExpect(StringTag))
            {
                return null;
            }

            if (cs.ItemsAvailable > 2 && cs.ExpectAt(1, StringTag) && cs.ExpectAt(2, StringTag))
            {
                return MultilineStringMatcher.TryMatch(cs);
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
