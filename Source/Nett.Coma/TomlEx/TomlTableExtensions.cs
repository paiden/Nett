namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nett.Extensions;

    internal static class TomlTableExtensions
    {
        public static void OverwriteWithValuesForLoadFrom(this TomlTable target, TomlTable from)
        {
            if (from == null) { throw new ArgumentNullException(nameof(from)); }

            // Loaded table type from file is more important than in memory table type
            // => always overwrite the target table type with the loaded stuff
            target.TableType = from.TableType;

            foreach (var r in from.Rows)
            {
                TomlObject targetObject = null;
                if (target.TryGetValue(r.Key, out targetObject)
                    && targetObject.TomlType == TomlObjectType.Table
                    && r.Value.TomlType == TomlObjectType.Table)
                {
                    ((TomlTable)targetObject).OverwriteWithValuesForLoadFrom((TomlTable)r.Value);
                }
                else
                {
                    target[r.Key] = r.Value;
                }
            }
        }

        public static void OverwriteWithValuesForSaveFrom(this TomlTable target, TomlTable from, bool addNewRows)
        {
            if (from == null) { throw new ArgumentNullException(nameof(from)); }

            var allRowKeys = new List<string>(target.Keys);
            foreach (var rowKey in allRowKeys)
            {
                TomlObject fromObject = null;
                if (from.TryGetValue(rowKey, out fromObject))
                {
                    TomlObject targetTable = target[rowKey] as TomlTable;

                    if (targetTable != null)
                    {
                        if (fromObject.TomlType == TomlObjectType.Table)
                        {
                            ((TomlTable)targetTable).OverwriteWithValuesForSaveFrom((TomlTable)fromObject, addNewRows);
                        }
                        else
                        {
                            throw new NotSupportedException($"Merging row with key '{rowKey}' of type {target[rowKey].ReadableTypeName} with row of type '{fromObject.ReadableTypeName}' is not supported.");
                        }
                    }
                    else
                    {
                        target[rowKey] = fromObject;
                    }
                }
                else if (addNewRows)
                {
                    target[rowKey] = fromObject;
                }
                else
                {
                    target.Remove(rowKey);
                }
            }
        }

        public static T ResolveKeyChain<T>(this TomlTable table, IList<string> keyChain)
            where T : TomlObject
        {
            var current = table.CheckNotNull(nameof(table));

            for (int i = 0; i < keyChain.Count - 1; i++)
            {
                current = (TomlTable)current[keyChain[i]];
            }

            return (T)current[keyChain.Last()];
        }

        public static TomlTable TransformToSourceTable(this TomlTable table, IConfigSource source, TomlObject rootSource = null)
        {
            table.CheckNotNull(nameof(table));

            var sourcesTable = rootSource == null
                ? Toml.Create(table.TableType)
                : TomlObjectFactory.CreateEmptyAttachedTable(rootSource, table.TableType);

            foreach (var r in table.Rows)
            {
                if (r.Value.TomlType == TomlObjectType.Table)
                {
                    sourcesTable[r.Key] = ((TomlTable)r.Value).TransformToSourceTable(source, sourcesTable);
                }
                else if (r.Value.TomlType == TomlObjectType.ArrayOfTables)
                {
                    var arr = (TomlTableArray)r.Value;
                    var sourcesArray = new TomlTableArray(table.Root, arr.Items.Select(t => t.TransformToSourceTable(source, sourcesTable)));
                    sourcesTable[r.Key] = sourcesArray;
                }
                else
                {
                    sourcesTable[r.Key] = new TomlSource(sourcesTable.Root, source);
                }
            }

            return sourcesTable;
        }

        public static TomlTable Clone(this TomlTable input)
            => input.CloneTableFor(input.Root);
    }
}
