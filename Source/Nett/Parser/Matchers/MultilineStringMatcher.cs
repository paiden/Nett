namespace Nett.Parser.Matchers
{
    using System.Linq;
    using System.Text;

    internal static class MultilineStringMatcher
    {
        private const string StringTag = "\"\"\"";

        public static Token? TryMatch(CharBuffer cs)
        {
            if (!cs.TryExpect(StringTag)) { return null; }

            var errPos = cs.FilePosition;

            StringBuilder sb = new StringBuilder(64);
            sb.Append(cs.Consume(StringTag.Length).ToArray());

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
                sb.Append(cs.Consume(3).ToArray());
                return new Token(TokenType.MultilineString, sb.ToString());
            }
        }
    }
}
