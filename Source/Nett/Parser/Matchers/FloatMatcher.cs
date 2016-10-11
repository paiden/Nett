namespace Nett.Parser.Matchers
{
    using System;
    using System.Text;

    internal static class FloatMatcher
    {
        internal static Token? Match(StringBuilder beforeFraction, CharBuffer cs)
        {
            if (cs.TryExpect('.'))
            {
                beforeFraction.Append(cs.Consume());
                var errPos = cs.FilePosition;
                var intFrac = IntMatcher.TryMatch(cs);

                if (!intFrac.HasValue)
                {
                    throw Parser.CreateParseError(errPos, "Fraction of float is missing.");
                }
                else if (intFrac.Value.type != TokenType.Integer && intFrac.Value.type != TokenType.Float)
                {
                    throw Parser.CreateParseError(errPos, $"Failed to read float because fraction '{intFrac.Value.value}' is invalid.");
                }

                beforeFraction.Append(intFrac.Value.value);
            }

            if (cs.TryExpect('e') || cs.TryExpect('E'))
            {
                beforeFraction.Append(cs.Consume());
                var errPos = cs.FilePosition;
                var intPar = IntMatcher.TryMatch(cs);
                if (!intPar.HasValue)
                {
                    throw Parser.CreateParseError(errPos, "Exponent of float is missing.");
                }
                else if (intPar.Value.type != TokenType.Integer)
                {
                    throw Parser.CreateParseError(errPos, $"Failed to read float because exponent '{intPar.Value.value}' is invalid.");
                }

                beforeFraction.Append(intPar.Value.value);
            }

            if (cs.TokenDone())
            {
                return new Token(TokenType.Float, beforeFraction.ToString());
            }
            else
            {
                throw Parser.CreateParseError(cs.FilePosition, "Failed to construct float token");
            }
        }
    }
}
