using System.Text;

namespace Nett.Parser.Matchers
{
    internal static class CommentMatcher
    {
        public static Token? TryMatch(LookaheadBuffer<char> chars)
        {
            if (!chars.TryExpect('#')) { return null; }
            else
            {
                chars.Consume();
                StringBuilder sb = new StringBuilder(64);
                while (!chars.End && !chars.TryExpect('\r') && !chars.TryExpect('\n'))
                {
                    sb.Append(chars.Consume());
                }

                return new Token(TokenType.Comment, sb.ToString());
            }
        }
    }
}
