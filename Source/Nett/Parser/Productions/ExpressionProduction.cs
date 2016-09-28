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
            var preComments = CommentProduction.TryParsePreExpressionCommenst(tokens);
            var expressionToken = tokens.Peek();

            var arrayKeyChain = TomlArrayTableProduction.TryApply(tokens);
            if (arrayKeyChain != null)
            {
                var addTo = GetTargetTable(root, arrayKeyChain, CreateImplicitelyType.Table);
                var arr = GetExistingOrCreateAndAdd(addTo, arrayKeyChain.Last(), errorPosition: expressionToken);

                arr.Comments.AddRange(preComments);
                arr.Comments.AddRange(CommentProduction.TryParseAppendExpressionComments(expressionToken, tokens));

                var newArrayEntry = new TomlTable(root);
                arr.Add(newArrayEntry);
                return newArrayEntry;
            }

            var tableKeyChain = TomlTableProduction.TryApply(tokens);
            if (tableKeyChain != null)
            {
                var newTable = new TomlTable(root) { IsDefined = true };
                newTable.Comments.AddRange(preComments);
                newTable.Comments.AddRange(CommentProduction.TryParseAppendExpressionComments(expressionToken, tokens));

                var addTo = GetTargetTable(root, tableKeyChain, CreateImplicitelyType.Table);

                string name = tableKeyChain.Last();
                var existingRow = addTo.TryGetValue(name);
                if (existingRow == null)
                {
                    addTo.Add(name, newTable);
                }
                else
                {
                    var tbl = existingRow as TomlTable;
                    if (tbl.IsDefined)
                    {
                        throw new Exception($"Failed to add new table because the target table already contains a row with the key '{name}' of type '{existingRow.ReadableTypeName}'.");
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
                    kvp.Item2.Comments.AddRange(preComments);
                    kvp.Item2.Comments.AddRange(CommentProduction.TryParseAppendExpressionComments(expressionToken, tokens));

                    current.Add(kvp.Item1, kvp.Item2);
                    return current;
                }
            }

            root.Comments.AddRange(preComments);

            return null;
        }

        private static TomlTableArray GetExistingOrCreateAndAdd(TomlTable target, string name, Token errorPosition)
        {
            TomlObject existing = target.TryGetValue(name);

            var typed = existing as TomlTableArray;

            if (existing != null && typed == null)
            {
                throw new InvalidOperationException(errorPosition.PrefixWithTokenPostion(
                    $"Cannot create array of tables with name '{name}' because there already is an row with that key of type '{existing.ReadableTypeName}'."));
            }
            else if (typed != null)
            {
                return typed;
            }

            var newTableArray = new TomlTableArray(target.Root);
            target.Add(name, newTableArray);

            return newTableArray;
        }

        private static TomlTable GetExistingOrCreateAndAddTable(TomlTable tbl, string key)
        {
            Func<TomlTable, TomlTable> createNew = (e) =>
                {
                    var newTable = new TomlTable(tbl.Root);
                    tbl.Add(key, newTable);
                    return newTable;
                };

            return GetExistinOrCreateAndAdd(tbl, key, createNew);
        }

        private static TomlTable GetExistingOrCreateAndAddTableArray(TomlTable tbl, string key)
        {
            Func<TomlTable, TomlTable> createNew = (e) =>
            {
                var array = new TomlTableArray(tbl.Root);
                var newTable = new TomlTable(tbl.Root);
                array.Add(newTable);
                tbl.Add(key, array);
                return newTable;
            };

            return GetExistinOrCreateAndAdd(tbl, key, createNew);
        }

        private static TomlTable GetExistinOrCreateAndAdd(TomlTable tbl, string key, Func<TomlTable, TomlTable> createNewItem)
        {
            var existingTargetTable = TryGetTableEntry(tbl.TryGetValue(key));
            if (existingTargetTable != null)
            {
                return existingTargetTable;
            }

            return createNewItem(tbl);
        }

        private static TomlTable GetTargetTable(TomlTable root, IList<string> keyChain, CreateImplicitelyType ct)
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
