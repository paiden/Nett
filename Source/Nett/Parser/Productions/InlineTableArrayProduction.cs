namespace Nett.Parser.Productions
{
    internal static class InlineTableArrayProduction
    {
        public static TomlTableArray TryApply(IMetaDataStore metaData, TokenBuffer tokens)
        {
            if (!tokens.TryExpectAt(0, TokenType.LBrac)) { return null; }
            if (!tokens.TryExpectAt(1, TokenType.LCurly)) { return null; }

            return Apply(metaData, tokens);
        }

        private static TomlTableArray Apply(IMetaDataStore metaData, TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);

            var arr = new TomlTableArray(metaData);
            TomlTable tbl = null;
            while ((tbl = InlineTableProduction.TryApply(metaData, tokens)) != null)
            {
                arr.Add(tbl);

                if (tokens.TryExpect(TokenType.Comma))
                {
                    tokens.Consume();
                }
                else
                {
                    tokens.Expect(TokenType.RBrac);
                }
            }

            tokens.ExpectAndConsume(TokenType.RBrac);

            return arr;
        }
    }
}
