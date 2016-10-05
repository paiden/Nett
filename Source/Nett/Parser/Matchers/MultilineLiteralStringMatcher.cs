namespace Nett.Parser.Matchers
{
    using System;
    using System.Linq;
    using System.Text;

    internal static class MultilineLiteralStringMatcher
    {
        private const string StringTag = "'''";

        internal static Token? TryMatch(CharBuffer cs)
        {
            StringBuilder sb = new StringBuilder(256);
            if (!cs.TryExpect(StringTag))
            {
                return null;
            }

            var errPos = cs.FilePosition;

            sb.Append(cs.Consume(3).ToArray());

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
                sb.Append(cs.Consume(StringTag.Length).ToArray());
                return new Token(TokenType.MultilineLiteralString, sb.ToString());
            }
        }
    }
}
