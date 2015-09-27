using System;

namespace Nett.Parser.Productions
{
    internal sealed class TomlTableProduction : Production<TomlTable>
    {
        private readonly TomlArrayTableProduction tableArray = new TomlArrayTableProduction();
        private readonly KeyValuePairProduction keyValuePair = new KeyValuePairProduction();

        public override TomlTable Apply(LookaheadBuffer<Token> tokens)
        {
            var table = new TomlTable();

            while (!tokens.End && tokens.Peek().type != TokenType.Eof)
            {
                var ta = this.tableArray.Apply(tokens);

                if (ta != null)
                {
                    throw new NotImplementedException();
                    //continue;
                }

                var kvp = keyValuePair.Apply(tokens);
                if (kvp != null)
                {
                    table.Add(kvp.Value.Key, kvp.Value.Value);
                    continue;
                }
            }

            return table;
        }
    }
}
