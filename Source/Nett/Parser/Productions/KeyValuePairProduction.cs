using System;
using System.Collections.Generic;

namespace Nett.Parser.Productions
{
    internal sealed class KeyValuePairProduction : Production<KeyValuePair<string, TomlObject>?>
    {
        private readonly KeyProduction keyProduction = new KeyProduction();
        private readonly ValueProduction valueProduction = new ValueProduction();

        public override KeyValuePair<string, TomlObject>? Apply(LookaheadBuffer<Token> tokens)
        {
            var key = this.keyProduction.Apply(tokens);

            if (tokens.Expect(TokenType.Assign))
            {
                tokens.Consume();
            }
            else
            {
                throw new Exception($"Failed to parse key value pair because expected '=' but '{tokens.Peek().value}' was found.");
            }

            var value = this.valueProduction.Apply(tokens);

            if (value == null)
            {
                throw new Exception($"Expected a value while parsing key value pair but value incompatible token '{tokens.Peek().value}' was found.");
            }

            return new KeyValuePair<string, TomlObject>(key, value);
        }
    }
}
