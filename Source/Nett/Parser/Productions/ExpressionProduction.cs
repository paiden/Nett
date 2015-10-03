namespace Nett.Parser.Productions
{
    internal sealed class ExpressionsProduction : Production<TomlTable>
    {
        private static readonly KeyValuePairProduction keyValueProduction = new KeyValuePairProduction();
        private static readonly TomlTableProduction tomlTableProduction = new TomlTableProduction();
        private static readonly TomlArrayTableProduction arrayTableProduction = new TomlArrayTableProduction();

        private TomlTable root;
        private TomlTable current;

        public ExpressionsProduction(TomlTable current, TomlTable root)
        {
            this.current = current;
            this.root = root;
        }

        public override TomlTable Apply(LookaheadBuffer<Token> tokens)
        {
            var arrayKey = arrayTableProduction.Apply(tokens);
            if (arrayKey != null)
            {
                string name = CalcArrayName(arrayKey);
                var addTo = CalculateTargetTable(this.root, arrayKey);
                var arr = GetExistingOrCreate(addTo, name);
                var newArrayEntry = new TomlTable();
                arr.Add(newArrayEntry);
                return newArrayEntry;
            }

            var tableKey = tomlTableProduction.Apply(tokens);
            if (tableKey != null)
            {
                var newTable = new TomlTable();
                var addTo = CalculateTargetTable(this.root, tableKey);
                addTo.Add(tableKey, newTable);
                return newTable;
            }

            var kvp = keyValueProduction.Apply(tokens);
            if (kvp != null)
            {
                current.Add(kvp.Item1, kvp.Item2);
                return current;
            }

            return null;
        }

        private static TomlTable CalculateTargetTable(TomlTable root, string key)
        {
            return root;
        }

        private static TomlTableArray GetExistingOrCreate(TomlTable target, string name)
        {
            return new TomlTableArray();
        }

        private static string CalcArrayName(string key)
        {
            return key;
        }
    }
}
