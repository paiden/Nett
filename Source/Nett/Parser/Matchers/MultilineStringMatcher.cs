namespace Nett.Parser.Matchers
{
    using System.Text;

    internal static class MultilineStringMatcher
    {
        private const string StringTag = "\"\"\"";

        public static Token? TryMatch(CharBuffer cs)
        {
            if (!cs.TryExpect(StringTag)) { return null; }

            var errPos = cs.FilePosition;

            StringBuilder sb = new StringBuilder(Constants.MatcherBufferSize);
            cs.Consume(StringTag.Length);

            while (!cs.TryExpect(StringTag))
            {
                if (cs.End)
                {
                    break;
                }

                if (cs.TryExpect('\\'))
                {
                    sb.Append(cs.Consume());
                }

                sb.Append(cs.Consume());
            }

            if (!cs.TryExpect(StringTag))
            {
                throw Parser.CreateParseError(errPos, Constants.ParseErrorStringNotClosed);
            }
            else
            {
                cs.Consume(StringTag.Length);
                return new Token(TokenType.MultilineString, sb.ToString());
            }
        }
    }
}
