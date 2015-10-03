namespace Nett.Parser.Productions
{
    internal static class KeyProduction
    {
        public static string Apply(TokenBuffer tokens) => ApplyInternal(tokens, required: true);
        public static string TryApply(TokenBuffer tokens) => ApplyInternal(tokens, required: false);

        private static string ApplyInternal(TokenBuffer tokens, bool required)
        {
            if (tokens.TryExpect(TokenType.BareKey)) { return tokens.Consume().value; }
            else if (tokens.TryExpect(TokenType.String)) { return tokens.Consume().value.Replace("\"", ""); }
            else if (required)
            {
                throw new System.Exception($"Failed to parse key because unexpected token '{tokens.Peek().value}' was found.");
            }
            else
            {
                return null;
            }
        }
    }
}
