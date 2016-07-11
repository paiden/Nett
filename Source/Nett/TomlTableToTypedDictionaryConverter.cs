namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using static System.Diagnostics.Debug;

    public class TomlTableToTypedDictionaryConverter : ITomlConverter
    {
        private static readonly Type DictType = typeof(IDictionary);

        public Type FromType => Types.TomlTableType;

        public bool CanConvertFrom(Type t) => t == Types.TomlTableType;

        public bool CanConvertTo(Type t) => DictType.IsAssignableFrom(t);

        public bool CanConvertToToml() => false;

        public object Convert(IMetaDataStore metaData, object value, Type targetType)
        {
            Assert(
                targetType != typeof(Dictionary<string, object>),
                $"Ensure '{nameof(TomlTableToDictionaryConverter)}' converter registered before this one.");

            var valueType = targetType.GetGenericArguments()[1];
            var target = (IDictionary)Activator.CreateInstance(targetType);

            TomlTable table = (TomlTable)value;
            foreach (var r in table.Rows)
            {
                target[r.Key] = r.Value.Get(valueType);
            }

            return target;
        }
    }
}
