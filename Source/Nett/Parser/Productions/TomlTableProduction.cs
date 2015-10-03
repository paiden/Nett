using System;

namespace Nett.Parser.Productions
{
    internal sealed class TomlTableProduction : Production<string>
    {
        private static readonly KeyProduction key = new KeyProduction();

        public override string Apply(LookaheadBuffer<Token> tokens)
        {
            if (!tokens.Expect(TokenType.LBrac))
            {
                return null;
            }

            tokens.Consume();

            string tableName = key.Apply(tokens);

            if (tableName == null)
            {
                throw new Exception();
            }

            if (!tokens.Expect(TokenType.RBrac))
            {
                throw new Exception();
            }

            tokens.Consume();

            return tableName;
        }
    }
}
