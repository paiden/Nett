namespace Nett.Parser.Matchers
{
    using System;
    using System.Text;

    internal static class FloatMatcher
    {
        internal static Token? TryMatch(StringBuilder beforeFraction, CharBuffer cs)
        {
            if (cs.TryExpect('.'))
            {
                beforeFraction.Append(cs.Consume());
                var intFrac = IntMatcher.TryMatch(cs);
                beforeFraction.Append(intFrac.Value.value);
            }

            if (cs.TryExpect('e') || cs.TryExpect('E'))
            {
                beforeFraction.Append(cs.Consume());
                var intPar = IntMatcher.TryMatch(cs);
                beforeFraction.Append(intPar.Value.value);
            }

            if (cs.TokenDone())
            {
                return new Token(TokenType.Float, beforeFraction.ToString());
            }
            else
            {
                throw new Exception("Failed to construct float token"); // TODO better error message.
            }
        }
    }
}
