namespace Nett.Parser.Productions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ExpressionsProduction
    {
        private enum CreateImplicitelyType
        {
            Table,
            ArrayOfTables,
        }

        public static TomlTable TryApply(TomlTable current, TomlTable.RootTable root, TokenBuffer tokens)
        {
            tokens.ConsumeAllNewlines();

            var preComments = CommentProduction.TryParsePreExpressionCommenst(tokens);
            var expressionToken = tokens.Peek();

            tokens.ConsumeAllNewlines();

            var arrayKeyChain = TomlArrayTableProduction.TryApply(tokens);
            if (arrayKeyChain != null)
            {
                var addTo = GetTargetTable(root, arrayKeyChain, CreateImplicitelyType.Table);
                var arr = GetExistingOrCreateAndAdd(addTo, arrayKeyChain.Last(), errorPosition: expressionToken);

                arr.AddComments(preComments);
                arr.AddComments(CommentProduction.TryParseAppendExpressionComments(expressionToken, tokens));

                var newArrayEntry = new TomlTable(root);
                arr.Add(newArrayEntry);
                return newArrayEntry;
            }

            var tableKeyChain = TomlTableProduction.TryApply(tokens);
            if (tableKeyChain != null)
            {
                var newTable = new TomlTable(root) { IsDefined = true };
                newTable.AddComments(preComments);
                newTable.AddComments(CommentProduction.TryParseAppendExpressionComments(expressionToken, tokens));

                var addTo = GetTargetTable(root, tableKeyChain, CreateImplicitelyType.Table);

                var key = tableKeyChain.Last();
                var existingRow = addTo.TryGetValue(key);
                if (existingRow == null)
                {
                    addTo.AddRow(key, newTable);
                }
                else
                {
                    var tbl = existingRow as TomlTable;
                    if (tbl.IsDefined)
                    {
                        throw new Exception($"Failed to add new table because the target table already contains a row with the key '{key}' of type '{existingRow.ReadableTypeName}'.");
                    }
                    else
                    {
                        tbl.IsDefined = true;
                        return tbl;
                    }
                }

                return newTable;
            }

            if (!tokens.End)
            {
                var kvp = KeyValuePairProduction.Apply(root, tokens);
                if (kvp != null)
                {
                    kvp.Item2.AddComments(preComments);
                    kvp.Item2.AddComments(CommentProduction.TryParseAppendExpressionComments(expressionToken, tokens));

                    current.AddRow(kvp.Item1, kvp.Item2);
                    return current;
                }
            }

            root.AddComments(preComments);

            return null;
        }

        private static TomlTableArray GetExistingOrCreateAndAdd(TomlTable target, TomlKey key, Token errorPosition)
        {
            TomlObject existing = target.TryGetValue(key);

            var typed = existing as TomlTableArray;

            if (existing != null && typed == null)
            {
                throw Parser.CreateParseError(
                    errorPosition,
                    $"Cannot create array of tables with key '{key}' because there already is an row with that key of type '{existing.ReadableTypeName}'.");
            }
            else if (typed != null)
            {
                return typed;
            }

            var newTableArray = new TomlTableArray(target.Root);
            target.AddRow(key, newTableArray);

            return newTableArray;
        }

        private static TomlTable GetExistingOrCreateAndAddTable(TomlTable tbl, TomlKey key)
        {
            Func<TomlTable, TomlTable> createNew = (e) =>
                {
                    var newTable = new TomlTable(tbl.Root);
                    tbl.AddRow(key, newTable);
                    return newTable;
                };

            return GetExistinOrCreateAndAdd(tbl, key, createNew);
        }

        private static TomlTable GetExistingOrCreateAndAddTableArray(TomlTable tbl, TomlKey key)
        {
            Func<TomlTable, TomlTable> createNew = (e) =>
            {
                var array = new TomlTableArray(tbl.Root);
                var newTable = new TomlTable(tbl.Root);
                array.Add(newTable);
                tbl.AddRow(key, array);
                return newTable;
            };

            return GetExistinOrCreateAndAdd(tbl, key, createNew);
        }

        private static TomlTable GetExistinOrCreateAndAdd(TomlTable tbl, TomlKey key, Func<TomlTable, TomlTable> createNewItem)
        {
            var existingTargetTable = TryGetTableEntry(tbl.TryGetValue(key));
            if (existingTargetTable != null)
            {
                return existingTargetTable;
            }

            return createNewItem(tbl);
        }

        private static TomlTable GetTargetTable(TomlTable root, IList<TomlKey> keyChain, CreateImplicitelyType ct)
        {
            var tgt = root;
            for (int i = 0; i < keyChain.Count - 1; i++)
            {
                tgt = ct == CreateImplicitelyType.Table
                    ? GetExistingOrCreateAndAddTable(tgt, keyChain[i])
                    : GetExistingOrCreateAndAddTableArray(tgt, keyChain[i]);
            }

            return tgt;
        }

        private static TomlTable TryGetTableEntry(TomlObject obj)
        {
            if (obj == null) { return null; }
            var tbl = obj as TomlTable;
            var arr = obj as TomlTableArray;

            return tbl ?? arr?.Last();
        }
    }
}
