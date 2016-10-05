namespace Nett.Parser.Matchers
{
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

            var errorPosition = cs.FilePosition;

            if (cs.TryExpectAt(1, StringTag) && cs.TryExpectAt(2, StringTag))
            {
                return MultilineStringMatcher.TryMatch(cs);
            }

            sb.Append(cs.Consume());

            while (!cs.End && !cs.TryExpect(StringTag))
            {
                if (cs.TryExpect('\\'))
                {
                    sb.Append(cs.Consume());
                }

                sb.Append(cs.Consume());
            }

            if (!cs.TryExpect(StringTag))
            {
                throw Parser.CreateParseError(errorPosition, Constants.ParseErrorStringNotClosed);
            }
            else
            {
                sb.Append(cs.Consume());
                return new Token(TokenType.String, sb.ToString());
            }
        }
    }
}
