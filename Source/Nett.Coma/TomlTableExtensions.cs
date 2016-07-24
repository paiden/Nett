namespace Nett.Coma
{
    using System;

    internal static class TomlTableExtensions
    {
        public static void OverwriteWithValuesFrom(this TomlTable target, TomlTable from)
        {
            if (from == null) { throw new ArgumentNullException("from"); }

            foreach (var r in from.Rows)
            {
                TomlObject targetObject = null;
                if (target.Rows.TryGetValue(r.Key, out targetObject)
                    && targetObject.TomlType == TomlObject.TomlObjectType.Table
                    && r.Value.TomlType == TomlObject.TomlObjectType.Table)
                {
                    ((TomlTable)targetObject).OverwriteWithValuesFrom((TomlTable)r.Value);
                }
                else
                {
                    target[r.Key] = r.Value;
                }
            }
        }
    }
}
