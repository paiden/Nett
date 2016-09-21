namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Nett.Extensions;
    using Toml;

    internal static class TomlTableExtensions
    {
        public static ImmutableTomlTable ToImmutable(this TomlTable table)
            => ToImmutable(table, new TPath());

        private static ImmutableTomlTable ToImmutable(this TomlTable table, TPath path)
        {
            var transformed = ImmutableTomlTable.Empty;

            foreach (var r in table.rows)
            {
                var key = path.WithKeyAdded(r.Key);
                switch (r.Value.TomlType)
                {
                    case TomlObject.TomlObjectType.Table:
                        var t = (TomlTable)r.Value;
                        transformed = transformed.WithMerged(t.ToImmutable(key));
                        break;
                    case TomlObject.TomlObjectType.Array:
                        var a = (TomlArray)r.Value;
                        var ia = new ImmutableTomlArray(a.MetaData, ImmutableArray.Create(a.Items));
                        transformed = transformed.WithRowAdded(key, ia);
                    case TomlObject.TomlObjectType.ArrayOfTables: throw new Exception();
                    default:
                        transformed = transformed.WithRowAdded(key, r.Value);
                }
            }
        }

        private static TomlObject TransformRow()

        public static void OverwriteWithValuesForLoadFrom(this TomlTable target, TomlTable from)
        {
            if (from == null) { throw new ArgumentNullException(nameof(from)); }

            foreach (var r in from.rows)
            {
                TomlObject targetObject = null;
                if (target.rows.TryGetValue(r.Key, out targetObject)
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

            var allRowKeys = new List<string>(target.rows.Keys);
            foreach (var rowKey in allRowKeys)
            {
                TomlObject fromObject = null;
                if (from.rows.TryGetValue(rowKey, out fromObject))
                {
                    TomlObject targetTable = target.rows[rowKey] as TomlTable;

                    if (targetTable != null)
                    {
                        if (fromObject.TomlType == TomlObject.TomlObjectType.Table)
                        {
                            ((TomlTable)targetTable).OverwriteWithValuesForSaveFrom((TomlTable)fromObject, addNewRows);
                        }
                        else
                        {
                            throw new NotSupportedException($"Merging row with key '{rowKey}' of type {target.rows[rowKey].ReadableTypeName} with row of type '{fromObject.ReadableTypeName}' is not supported.");
                        }
                    }
                    else
                    {
                        target.rows[rowKey] = fromObject;
                    }
                }
                else if (addNewRows)
                {
                    target[rowKey] = fromObject;
                }
                else
                {
                    target.rows.Remove(rowKey);
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
            foreach (var r in table.rows)
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
