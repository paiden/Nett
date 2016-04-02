namespace Nett.Parser.Productions
{
    internal static class InlineTableArrayProduction
    {
        public static TomlTableArray TryApply(TokenBuffer tokens)
        {
            if (!tokens.TryExpectAt(0, TokenType.LBrac)) { return null; }
            if (!tokens.TryExpectAt(1, TokenType.LCurly)) { return null; }

            return Apply(tokens);
        }

        private static TomlTableArray Apply(TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);

            var arr = new TomlTableArray();
            TomlTable tbl = null;
            while ((tbl = InlineTableProduction.TryApply(tokens)) != null)
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
