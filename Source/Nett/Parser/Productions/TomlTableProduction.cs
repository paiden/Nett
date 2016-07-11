namespace Nett.Parser.Productions
{
    using System.Collections.Generic;

    internal static class TomlTableProduction
    {
        public static IList<string> Apply(TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            IList<string> tableKeyChain = TableKeyProduction.Apply(tokens);
            tokens.ExpectAndConsume(TokenType.RBrac);
            return tableKeyChain;
        }

        public static IList<string> TryApply(TokenBuffer tokens) => !tokens.TryExpect(TokenType.LBrac) ? null : Apply(tokens);
    }
}
