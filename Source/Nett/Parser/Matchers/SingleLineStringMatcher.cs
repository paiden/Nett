using System;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal abstract class SingleLineStringMatcher : MatcherBase
    {
        private readonly char StringTag;
        private readonly TokenType TokenType;

        public SingleLineStringMatcher(char stringTag, TokenType tokenType)
        {
            this.StringTag = stringTag;
            this.TokenType = tokenType;
        }

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(256);
            if (!cs.Expect(StringTag))
            {
                return NoMatch;
            }

            sb.Append(cs.Consume());

            while (cs.Peek() != StringTag)
            {
                if (cs.Expect('\\'))
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
                sb.Append(cs.Consume());
                return new Token(this.TokenType, sb.ToString());
            }
        }
    }
}
