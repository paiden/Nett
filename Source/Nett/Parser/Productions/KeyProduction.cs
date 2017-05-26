namespace Nett.Parser.Productions
{
    internal static class KeyProduction
    {
        public static TomlKey Apply(TokenBuffer tokens) => ApplyInternal(tokens, required: true);

        public static TomlKey TryApply(TokenBuffer tokens) => ApplyInternal(tokens, required: false);

        private static TomlKey ApplyInternal(TokenBuffer tokens, bool required)
        {
            if (tokens.TryExpect(TokenType.BareKey) || tokens.TryExpect(TokenType.Integer))
            {
                return new TomlKey(tokens.Consume().value, TomlKey.KeyType.Bare);
            }
            else if (tokens.TryExpect(TokenType.String))
            {
                return new TomlKey(tokens.Consume().value, TomlKey.KeyType.Basic);
            }
            else if (tokens.TryExpect(TokenType.LiteralString))
            {
                return new TomlKey(tokens.Consume().value, TomlKey.KeyType.Literal);
            }
            else if (required)
            {
                var t = tokens.Peek();
                if (t.value == "=")
                {
                    throw Parser.CreateParseError(t, "Key is missing.");
                }
                else
                {
                    throw Parser.CreateParseError(t, $"Failed to parse key because unexpected token '{t.value}' was found.");
                }
            }
            else
            {
                return new TomlKey(string.Empty);
            }
        }
    }
}
