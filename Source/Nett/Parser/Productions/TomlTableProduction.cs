using System.Collections.Generic;

namespace Nett.Parser.Productions
{
    internal static class TomlTableProduction
    {
        public static IList<string> TryApply(LookaheadBuffer<Token> tokens) => !tokens.TryExpect(TokenType.LBrac) ? null : Apply(tokens);

        public static IList<string> Apply(LookaheadBuffer<Token> tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            IList<string> tableKeyChain = TableKeyProduction.Apply(tokens);
            tokens.ExpectAndConsume(TokenType.RBrac);
            return tableKeyChain;
        }
    }
}
