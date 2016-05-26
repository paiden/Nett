using System;

namespace Nett.Parser.Productions
{
    internal static class KeyValuePairProduction
    {
        public static Tuple<string, TomlObject> Apply(IMetaDataStore metaData, TokenBuffer tokens)
        {
            var key = KeyProduction.Apply(tokens);

            tokens.ExpectAndConsume(TokenType.Assign);

            var inlineTableArray = InlineTableArrayProduction.TryApply(metaData, tokens);
            if (inlineTableArray != null)
            {
                return new Tuple<string, TomlObject>(key, inlineTableArray);
            }

            var inlineTable = InlineTableProduction.TryApply(metaData, tokens);
            if (inlineTable != null)
            {
                return new Tuple<string, TomlObject>(key, inlineTable);
            }

            var value = ValueProduction.Apply(metaData, tokens);
            return Tuple.Create(key, value);
        }
    }
}
