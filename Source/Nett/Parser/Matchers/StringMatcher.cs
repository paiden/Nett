using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal sealed class StringMatcher : MatcherBase
    {
        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(256);
            if (!cs.PeekIs('\"'))
            {
                return NoMatch;
            }

            sb.Append(cs.Consume());

            while (cs.Peek() != '\"')
            {
                sb.Append(cs.Consume());
            }

            if (cs.Peek() != '\"')
            {
                throw new Exception($"Closing '\"' not found for string {sb.ToString()}'");
            }
            else
            {
                sb.Append(cs.Consume());
                return new Token(TokenType.NormalString, sb.ToString());
            }
        }
    }
}
