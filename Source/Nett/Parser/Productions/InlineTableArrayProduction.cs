namespace Nett.Parser.Productions
{
    internal static class InlineTableArrayProduction
    {
        public static TomlTableArray TryApply(ITomlRoot root, TokenBuffer tokens)
        {
            var ictx = tokens.GetImaginaryContext();

            if (!ictx.TryExpectAndConsume(TokenType.LBrac)) { return null; }
            ictx.ConsumeAllNewlines();
            if (!ictx.TryExpect(TokenType.LCurly)) { return null; }

            ictx.MakeItReal();

            return Apply(root, tokens, true);
        }

        private static TomlTableArray Apply(ITomlRoot root, TokenBuffer tokens, bool withoutLBrac = false)
        {
            if (!withoutLBrac)
            {
                tokens.ExpectAndConsume(TokenType.LBrac);
                tokens.ConsumeAllNewlines();
            }

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
