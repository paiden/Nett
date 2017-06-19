namespace Nett.Parser.Matchers
{
    using System.Text;

    internal static class MultilineLiteralStringMatcher
    {
        private const string StringTag = "'''";

        internal static Token? TryMatch(CharBuffer cs)
        {
            StringBuilder sb = new StringBuilder(Constants.MatcherBufferSize);
            if (!cs.TryExpect(StringTag))
            {
                return null;
            }

            var errPos = cs.FilePosition;

            cs.Consume(StringTag.Length);
            cs.TryTrimNewline();

            while (!cs.End && !cs.TryExpect(StringTag))
            {
                sb.Append(cs.Consume());
            }

            if (!cs.TryExpect(StringTag))
            {
                throw Parser.CreateParseError(errPos, Constants.ParseErrorStringNotClosed);
            }
            else
            {
                cs.Consume(StringTag.Length);
                return new Token(TokenType.MultilineLiteralString, sb.ToString());
            }
        }
    }
}
