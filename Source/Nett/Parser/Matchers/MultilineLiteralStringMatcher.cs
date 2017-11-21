using System.Text;
using static System.Diagnostics.Debug;

namespace Nett.Parser.Matchers
{
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
                StringBuilder closeSb = new StringBuilder(8);
                while (cs.TryExpect("'"))
                {
                    closeSb.Append(cs.Consume());
                }

                Assert(
                    closeSb.Length >= StringTag.Length,
                    "Should be ensured by the while clause above, check the implementation.");

                sb.Append(new string('\'', closeSb.Length - StringTag.Length));

                return new Token(TokenType.MultilineLiteralString, sb.ToString());
            }
        }
    }
}
