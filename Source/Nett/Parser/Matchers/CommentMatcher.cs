namespace Nett.Parser.Matchers
{
    using System.Text;

    internal static class CommentMatcher
    {
        public static Token? TryMatch(LookaheadBuffer<char> chars)
        {
            if (!chars.TryExpect('#')) { return null; }
            else
            {
                chars.Consume();
                StringBuilder sb = new StringBuilder(Constants.MatcherBufferSize);
                while (!chars.End && !chars.TryExpect('\r') && !chars.TryExpect('\n'))
                {
                    sb.Append(chars.Consume());
                }

                return new Token(TokenType.Comment, sb.ToString());
            }
        }
    }
}
