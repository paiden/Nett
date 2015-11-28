using System;

namespace Nett.Parser.Productions
{
    internal static class KeyValuePairProduction
    {
        public static Tuple<string, TomlObject> Apply(TokenBuffer tokens)
        {
            var key = KeyProduction.Apply(tokens);

            tokens.ExpectAndConsume(TokenType.Assign);

            var inlineTable = InlineTableProduction.TryApply(tokens);
            if (inlineTable != null)
            {
                return new Tuple<string, TomlObject>(key, inlineTable);
            }

            var value = ValueProduction.Apply(tokens);
            return Tuple.Create(key, value);
        }
    }
}
