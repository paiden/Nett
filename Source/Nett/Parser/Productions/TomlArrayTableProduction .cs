using System;

namespace Nett.Parser.Productions
{
    internal sealed class TomlArrayTableProduction : Production<string>
    {
        private static readonly KeyProduction keyProduction = new KeyProduction();

        public override string Apply(LookaheadBuffer<Token> tokens)
        {
            if (!tokens.ExpectAt(0, TokenType.LBrac)) { return null; }
            if (!tokens.ExpectAt(1, TokenType.RBrac)) { return null; }

            tokens.Consume();
            tokens.Consume();

            var key = keyProduction.Apply(tokens);

            if (key == null) { throw new Exception(); }

            if (!tokens.ExpectAt(0, TokenType.RBrac)) { throw new Exception(); }
            if (!tokens.ExpectAt(1, TokenType.RBrac)) { throw new Exception(); }

            tokens.Consume();
            tokens.Consume();

            return key;
        }
    }
}
