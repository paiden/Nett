namespace Nett.Parser.Productions
{
    internal static class InlineTableProduction
    {
        public static TomlTable Apply(ITomlRoot root, TokenBuffer tokens)
        {
            TomlTable inlineTable = new TomlTable(root, TomlTable.TableTypes.Inline);

            tokens.ExpectAndConsume(TokenType.LCurly);

            if (!tokens.TryExpect(TokenType.RBrac))
            {
                var kvp = KeyValuePairProduction.Apply(root, tokens);
                inlineTable.AddRow(kvp.Item1, kvp.Item2);

                while (tokens.TryExpect(TokenType.Comma))
                {
                    tokens.Consume();
                    kvp = KeyValuePairProduction.Apply(root, tokens);
                    inlineTable.AddRow(kvp.Item1, kvp.Item2);
                }
            }

            tokens.ExpectAndConsume(TokenType.RCurly);
            return inlineTable;
        }

        public static TomlTable TryApply(ITomlRoot root, TokenBuffer tokens) =>
                    tokens.TryExpect(TokenType.LCurly)
                ? Apply(root, tokens)
                : null;
    }
}
