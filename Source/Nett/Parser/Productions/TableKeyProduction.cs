using System.Collections.Generic;

namespace Nett.Parser.Productions
{
    internal static class TableKeyProduction
    {
        public static IList<string> Apply(TokenBuffer tokens)
        {
            List<string> keyChain = new List<string>();
            var key = KeyProduction.Apply(tokens);
            keyChain.Add(key);

            while (tokens.TryExpect(TokenType.Dot))
            {
                tokens.Consume();
                keyChain.Add(KeyProduction.TryApply(tokens));
            }

            return keyChain;
        }
    }
}
