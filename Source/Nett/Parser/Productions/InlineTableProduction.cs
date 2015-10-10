namespace Nett.Parser.Productions
{
    internal static class InlineTableProduction
    {
        public static TomlTable Apply(TokenBuffer tokens)
        {
            TomlTable inlineTable = new TomlTable();

            tokens.ExpectAndConsume(TokenType.LCurly);

            if (!tokens.TryExpect(TokenType.RBrac))
            {
                var kvp = KeyValuePairProduction.Apply(tokens);
                inlineTable.Add(kvp.Item1, kvp.Item2);

                while (tokens.TryExpect(TokenType.Comma))
                {
                    tokens.Consume();
                    kvp = KeyValuePairProduction.Apply(tokens);
                    inlineTable.Add(kvp.Item1, kvp.Item2);
                }
            }

            tokens.ExpectAndConsume(TokenType.RCurly);
            return inlineTable;
        }
    }
}
