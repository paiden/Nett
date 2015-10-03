namespace Nett.Parser.Matchers
{
    internal sealed class BoolMatcher : MatcherBase
    {
        private const string T = "true";
        private const string F = "false";

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            if (cs.Expect(T))
            {
                cs.Consume(T.Length);
                return new Token(TokenType.Bool, T);
            }
            else if (cs.Expect(F))
            {
                cs.Consume(F.Length);
                return new Token(TokenType.Bool, F);
            }
            else
            {
                return new Token?();
            }
        }
    }
}
