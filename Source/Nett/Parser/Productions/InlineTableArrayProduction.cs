namespace Nett.Parser.Productions
{
    internal static class InlineTableArrayProduction
    {
        public static TomlTableArray TryApply(ITomlRoot root, TokenBuffer tokens)
        {
            if (!tokens.TryExpectAt(0, TokenType.LBrac)) { return null; }
            if (!tokens.TryExpectAt(1, TokenType.LCurly)) { return null; }

            return Apply(root, tokens);
        }

        private static TomlTableArray Apply(ITomlRoot root, TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            tokens.ConsumeAllNewlines();

            var arr = new TomlTableArray(root);
            TomlTable tbl = null;
            while ((tbl = InlineTableProduction.TryApply(root, tokens)) != null)
            {
                arr.Add(tbl);

                if (tokens.TryExpect(TokenType.Comma))
                {
                    tokens.Consume();
                    tokens.ConsumeAllNewlines();
                }
                else
                {
                    tokens.ConsumeAllNewlines();
                    tokens.Expect(TokenType.RBrac);
                }
            }

            tokens.ConsumeAllNewlines();
            tokens.ExpectAndConsume(TokenType.RBrac);

            return arr;
        }
    }
}
