namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;

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

        public static void OverwriteWithValuesForSaveFrom(this TomlTable target, TomlTable from)
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
                            ((TomlTable)targetTable).OverwriteWithValuesForSaveFrom((TomlTable)fromObject);
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
                else
                {
                    target.Rows.Remove(rowKey);
                }
            }
        }
    }
}
