using System;

namespace Nett.Parser.Productions
{
    internal static class KeyValuePairProduction
    {
        public static Tuple<string, TomlObject> Apply(LookaheadBuffer<Token> tokens)
        {
            var key = KeyProduction.Apply(tokens);
            tokens.ExpectAndConsume(TokenType.Assign);
            var value = ValueProduction.Apply(tokens);
            return Tuple.Create(key, value);
        }
    }
}
