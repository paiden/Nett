namespace Nett.Parser.Productions
{
    using System.Collections.Generic;

    internal static class TomlTableProduction
    {
        public static IList<TomlKey> Apply(TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            IList<TomlKey> tableKeyChain = TableKeyProduction.Apply(tokens);
            tokens.ExpectAndConsume(TokenType.RBrac);

            if (!tokens.TryExpectAndConsume(TokenType.NewLine) && !tokens.TryExpectAndConsume(TokenType.Comment) && !tokens.End)
            {
                var msg = $"Expected newline after table specifier. "
                    + $"Token of type '{tokens.Peek().type}' with value '{tokens.Peek().value}' on same line.";
                throw Parser.CreateParseError(tokens.Peek(), msg);
            }

            return tableKeyChain;
        }

        public static IList<TomlKey> TryApply(TokenBuffer tokens) => !tokens.TryExpect(TokenType.LBrac) ? null : Apply(tokens);
    }
}
