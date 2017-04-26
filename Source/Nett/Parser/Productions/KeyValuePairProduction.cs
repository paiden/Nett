namespace Nett.Parser.Productions
{
    using System;

    internal static class KeyValuePairProduction
    {
        public static Tuple<TomlKey, TomlObject> Apply(ITomlRoot root, TokenBuffer tokens)
        {
            var key = KeyProduction.Apply(tokens);

            tokens.ExpectAndConsume(TokenType.Assign);

            var inlineTableArray = InlineTableArrayProduction.TryApply(root, tokens);
            if (inlineTableArray != null)
            {
                return new Tuple<TomlKey, TomlObject>(key, inlineTableArray);
            }

            var inlineTable = InlineTableProduction.TryApply(root, tokens);
            if (inlineTable != null)
            {
                return new Tuple<TomlKey, TomlObject>(key, inlineTable);
            }

            var value = ValueProduction.Apply(root, tokens);
            return Tuple.Create(key, value);
        }
    }
}
