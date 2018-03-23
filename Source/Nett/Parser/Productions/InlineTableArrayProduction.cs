namespace Nett.Parser.Productions
{
    internal static class InlineTableArrayProduction
    {
        public static TomlTableArray TryApply(ITomlRoot root, TokenBuffer tokens)
        {
            var ictx = tokens.GetImaginaryContext();

            if (!ictx.TryExpectAndConsume(TokenType.LBrac)) { return null; }
            ictx.ConsumeAllNewlinesAndComments();
            if (!ictx.TryExpect(TokenType.LCurly)) { return null; }

            return Apply(root, tokens);
        }

        private static TomlTableArray Apply(ITomlRoot root, TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            tokens.ConsumeAllNewlines();

            var prep = CommentProduction.TryParseComments(tokens, CommentLocation.Prepend);

            var arr = new TomlTableArray(root);
            while (true)
            {
                var tbl = InlineTableProduction.TryApply(root, tokens);
                if (tbl == null)
                {
                    break;
                }

                if (prep != null)
                {
                    tbl.AddComments(prep);
                    prep = null;
                }

                arr.Add(tbl);

                if (tokens.TryExpect(TokenType.Comma))
                {
                    tokens.Consume();
                    tokens.ConsumeAllNewlines();
                    tbl.AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));
                }
                else
                {
                    break;
                }
            }

            tokens.ConsumeAllNewlines();

            if (arr.Count > 0)
            {
                arr.Last().AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));
            }
            else
            {
                arr.AddComments(prep);
            }

            tokens.ExpectAndConsume(TokenType.RBrac);
            arr.AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));

            return arr;
        }
    }
}
