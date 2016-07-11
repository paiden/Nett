namespace Nett.Parser.Productions
{
    internal static class InlineTableProduction
    {
        public static TomlTable Apply(IMetaDataStore metaData, TokenBuffer tokens)
        {
            TomlTable inlineTable = new TomlTable(metaData, TomlTable.TableTypes.Inline);

            tokens.ExpectAndConsume(TokenType.LCurly);

            if (!tokens.TryExpect(TokenType.RBrac))
            {
                var kvp = KeyValuePairProduction.Apply(metaData, tokens);
                inlineTable.Add(kvp.Item1, kvp.Item2);

                while (tokens.TryExpect(TokenType.Comma))
                {
                    tokens.Consume();
                    kvp = KeyValuePairProduction.Apply(metaData, tokens);
                    inlineTable.Add(kvp.Item1, kvp.Item2);
                }
            }

            tokens.ExpectAndConsume(TokenType.RCurly);
            return inlineTable;
        }

        public static TomlTable TryApply(IMetaDataStore metaData, TokenBuffer tokens) =>
                    tokens.TryExpect(TokenType.LCurly)
                ? Apply(metaData, tokens)
                : null;
    }
}
