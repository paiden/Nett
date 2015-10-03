using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nett.Parser.Productions
{
    internal sealed class ExpressionsProduction : Production<TomlTable>
    {
        private static readonly KeyValuePairProduction keyValueProduction = new KeyValuePairProduction();

        private TomlTable root;
        private TomlTable current;

        public ExpressionsProduction(TomlTable current, TomlTable root)
        {
            this.current = current;
            this.root = root;
        }

        public override TomlTable Apply(LookaheadBuffer<Token> tokens)
        {
            var arrayKeyChain = TomlArrayTableProduction.TryApply(tokens);
            if (arrayKeyChain != null)
            {
                var addTo = CalculateTargetTable(this.root, arrayKeyChain);
                var arr = GetExistingOrCreateAndAdd(addTo, arrayKeyChain.Last());
                var newArrayEntry = new TomlTable();
                arr.Add(newArrayEntry);
                return newArrayEntry;
            }

            var tableKeyChain = TomlTableProduction.TryApply(tokens);
            if (tableKeyChain != null)
            {
                var newTable = new TomlTable();
                var addTo = GetTargetTableForTable(this.root, tableKeyChain);
                addTo.Add(tableKeyChain.Last(), newTable);
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

        private static TomlTable GetTargetTableForTable(TomlTable root, IList<string> keyChain)
        {
            var tgt = root;
            for (int i = 0; i < keyChain.Count - 1; i++)
            {
                tgt = GetExistingTableOrCreateAndAdd(tgt, keyChain[i]);
            }

            return tgt;
        }

        private static TomlTable GetExistingTableOrCreateAndAdd(TomlTable tbl, string key)
        {
            var existing = tbl.TryGet<TomlTable>(key);
            if (existing != null)
            {
                return existing;
            }

            var newTable = new TomlTable();
            tbl.Add(key, newTable);
            return newTable;
        }

        private static TomlTable CalculateTargetTable(TomlTable root, IList<string> keyChain)
        {
            var tgt = root;
            for (int i = 0; i < keyChain.Count - 1; i++)
            {
                var tmp = tgt.Get(keyChain[i]);
                var tbl = tmp as TomlTable;
                var arr = tmp as TomlTableArray;

                Debug.Assert(tbl != null || arr != null);

                tgt = tbl ?? arr.Last();
            }

            return tgt;
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
