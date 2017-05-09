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

        public TomlObjectType? TomlTargetType => null;

        public bool CanConvertFrom(Type t) => t == Types.TomlTableType;

        public bool CanConvertTo(Type t) => DictType.IsAssignableFrom(t);

        public bool CanConvertToToml() => false;

        public object Convert(ITomlRoot root, object value, Type targetType)
        {
            Assert(
                targetType != typeof(Dictionary<string, object>),
                $"Ensure '{nameof(TomlTableToDictionaryConverter)}' converter registered before this one.");

            var valueType = FindDictionaryValueType(targetType);
            var target = (IDictionary)Activator.CreateInstance(targetType);

            TomlTable table = (TomlTable)value;
            foreach (var r in table.Rows)
            {
                target[r.Key] = r.Value.Get(valueType);
            }

            return target;
        }

        private static Type FindDictionaryValueType(Type sourceType)
        {
            if (sourceType == null) { return typeof(object); }

            var ga = sourceType.GetGenericArguments();
            if (ga.Length == 2 && sourceType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return ga[1];
            }

            return FindDictionaryValueType(sourceType.BaseType);
        }
    }
}
