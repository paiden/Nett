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

            foreach (var r in from.Rows)
            {
                TomlObject targetObject = null;
                if (target.Rows.TryGetValue(r.Key, out targetObject)
                    && targetObject.TomlType == TomlObject.TomlObjectType.Table
                    && r.Value.TomlType == TomlObject.TomlObjectType.Table)
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

            var allRowKeys = new List<string>(target.Rows.Keys);
            foreach (var rowKey in allRowKeys)
            {
                TomlObject fromObject = null;
                if (from.Rows.TryGetValue(rowKey, out fromObject))
                {
                    TomlObject targetTable = target.Rows[rowKey] as TomlTable;

                    if (targetTable != null)
                    {
                        if (fromObject.TomlType == TomlObject.TomlObjectType.Table)
                        {
                            ((TomlTable)targetTable).OverwriteWithValuesForSaveFrom((TomlTable)fromObject, addNewRows);
                        }
                        else
                        {
                            throw new NotSupportedException($"Merging row with key '{rowKey}' of type {target.Rows[rowKey].ReadableTypeName} with row of type '{fromObject.ReadableTypeName}' is not supported.");
                        }
                    }
                    else
                    {
                        target.Rows[rowKey] = fromObject;
                    }
                }
                else if (addNewRows)
                {
                    target[rowKey] = fromObject;
                }
                else
                {
                    target.Rows.Remove(rowKey);
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

        public static TomlTable TransformToSourceTable(this TomlTable table, IConfigSource source)
        {
            table.CheckNotNull(nameof(table));

            var sourcesTable = Toml.Create();
            foreach (var r in table.Rows)
            {
                if (r.Value.TomlType == TomlObject.TomlObjectType.Table)
                {
                    sourcesTable[r.Key] = ((TomlTable)r.Value).TransformToSourceTable(source);
                }
                else if (r.Value.TomlType == TomlObject.TomlObjectType.ArrayOfTables)
                {
                    var arr = (TomlTableArray)r.Value;
                    var sourcesArray = new TomlTableArray(table.MetaData, arr.Items.Select(t => t.TransformToSourceTable(source)));
                    sourcesTable[r.Key] = sourcesArray;
                }
                else
                {
                    sourcesTable[r.Key] = new TomlSource(table.MetaData, source);
                }
            }

            return sourcesTable;
        }
    }
}
