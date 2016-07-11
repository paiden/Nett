namespace Nett.Parser.Matchers
{
    internal static class BoolMatcher
    {
        private const string F = "false";
        private const string T = "true";

        public static Token? TryMatch(CharBuffer cs)
        {
            if (cs.TryExpect(T))
            {
                cs.Consume(T.Length);
                return new Token(TokenType.Bool, T);
            }
            else if (cs.TryExpect(F))
            {
                cs.Consume(F.Length);
                return new Token(TokenType.Bool, F);
            }
            else
            {
                return null;
            }
        }
    }
}
