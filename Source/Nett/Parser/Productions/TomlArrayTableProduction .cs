using System.Collections.Generic;

namespace Nett.Parser.Productions
{
    internal static class TomlArrayTableProduction
    {
        public static IList<string> TryApply(TokenBuffer tokens)
        {
            if (!tokens.TryExpectAt(0, TokenType.LBrac)) { return null; }
            if (!tokens.TryExpectAt(1, TokenType.LBrac)) { return null; }

            return Apply(tokens);
        }

        public static IList<string> Apply(TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            tokens.ExpectAndConsume(TokenType.LBrac);

            var key = TableKeyProduction.Apply(tokens);

            tokens.ExpectAndConsume(TokenType.RBrac);
            tokens.ExpectAndConsume(TokenType.RBrac);

            return key;
        }
    }
}
