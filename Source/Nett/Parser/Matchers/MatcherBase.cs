using System.Text;

namespace Nett.Parser.Matchers
{
    internal abstract class MatcherBase
    {
        internal virtual Token? Match(LookaheadBuffer<char> cs)
        {
            StringBuilder sb = new StringBuilder(64);

            while (cs.HasNext())
            {
                sb.Append(cs.Consume());
            }

            return new Token(TokenType.Unknown, sb.ToString());
        }

    }
}
