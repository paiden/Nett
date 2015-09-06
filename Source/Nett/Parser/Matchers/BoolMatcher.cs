namespace Nett.Parser.Matchers
{
    internal sealed class BoolMatcher : MatcherBase
    {
        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            if (cs.Expect("true"))
            {
                cs.Consume(4);
                return new Token(TokenType.Bool, "true");
            }
            else if (cs.Expect("false"))
            {
                cs.Consume(4);
                return new Token(TokenType.Bool, "false");
            }
            else
            {
                return new Token?();
            }
        }
    }
}
