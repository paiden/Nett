using System.Collections.Generic;

namespace Nett.Parser.Productions
{
    internal static class CommentProduction
    {
        public static IList<TomlComment> TryParsePreExpressionCommenst(TokenBuffer tokens)
        {
            var comments = new List<TomlComment>();
            while (tokens.TryExpect(TokenType.Comment))
            {
                comments.Add(new TomlComment(tokens.Consume().value, CommentLocation.Prepend));
            }

            return comments;
        }

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
            }

            return comments;
        }
    }
}
