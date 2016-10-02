namespace Nett.Parser.Productions
{
    using System.Collections.Generic;

    internal static class CommentProduction
    {
        public static IList<TomlComment> TryParseAppendExpressionComments(Token lastExpressionToken, TokenBuffer tokens)
        {
            var comments = new List<TomlComment>();
            while (tokens.TryExpect(TokenType.Comment) && tokens.Peek().line == lastExpressionToken.line)
            {
                comments.Add(new TomlComment(tokens.Consume().value, CommentLocation.Append));
            }

            return comments;
        }

        public static IList<TomlComment> TryParseComments(TokenBuffer tokens, CommentLocation location)
        {
            var comments = new List<TomlComment>();
            while (tokens.TryExpect(TokenType.Comment))
            {
                comments.Add(new TomlComment(tokens.Consume().value, location));
                tokens.ConsumeAllNewlines();
            }

            tokens.ConsumeAllNewlines();

            return comments;
        }

        public static IList<TomlComment> TryParsePreExpressionCommenst(TokenBuffer tokens)
        {
            var comments = new List<TomlComment>();
            while (tokens.TryExpect(TokenType.Comment))
            {
                comments.Add(new TomlComment(tokens.Consume().value, CommentLocation.Prepend));
                tokens.ConsumeAllNewlines();
            }

            tokens.ConsumeAllNewlines();

            return comments;
        }
    }
}
