using System;
using System.Linq;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal static class MultilineStringMatcher
    {
        private const string StringTag = "\"\"\"";

        public static Token? TryMatch(LookaheadBuffer<char> cs)
        {
            if (!cs.Expect(StringTag)) { return null; }

            StringBuilder sb = new StringBuilder(64);
            sb.Append(cs.Consume(StringTag.Length).ToArray());

            while (!cs.Expect(StringTag))
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

            if (!cs.Expect(StringTag))
            {
                throw new Exception($"Closing '{StringTag}' not found for string {sb.ToString()}'");
            }
            else
            {
                sb.Append(cs.Consume(3).ToArray());
                return new Token(TokenType.MultilineString, sb.ToString());
            }
        }
    }
}
