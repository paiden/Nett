using System;

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
                var arr = GetExistingOrCreateAndAdd(addTo, name);
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

        private static TomlTableArray GetExistingOrCreateAndAdd(TomlTable target, string name)
        {
            TomlObject existing = null;
            target.Rows.TryGetValue(name, out existing);

            var typed = existing as TomlTableArray;

            if (existing != null && typed == null)
            {
                throw new InvalidOperationException($"Cannot create array of tables with name '{name}' because there already is an row with that key of type '{existing.ReadableTypeName}'.");
            }
            else if (typed != null)
            {
                return typed;
            }

            var newTableArray = new TomlTableArray();
            target.Add(name, newTableArray);

            return newTableArray;
        }

        private static string CalcArrayName(string key)
        {
            return key;
        }
    }
}
