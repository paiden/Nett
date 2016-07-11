namespace Nett.Parser.Matchers
{
    using System;
    using System.Text;

    internal static class StringMatcher
    {
        private const char StringTag = '\"';

        internal static Token? TryMatch(CharBuffer cs)
        {
            StringBuilder sb = new StringBuilder(128);
            if (!cs.TryExpect(StringTag))
            {
                return null;
            }

            if (cs.TryExpectAt(1, StringTag) && cs.TryExpectAt(2, StringTag))
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
