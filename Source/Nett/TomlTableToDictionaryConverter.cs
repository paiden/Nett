using System;
using System.Collections;
using System.Collections.Generic;
using Nett.Extensions;
using Nett.Util;

namespace Nett
{
    internal sealed class TomlTableToDictionaryConverter : ITomlConverter
    {
        private static readonly Type DictType = typeof(IDictionary);

        public Type FromType => Types.TomlTableType;

        public TomlObjectType? TomlTargetType => null;

        public bool CanConvertFrom(Type t) => t == Types.TomlTableType;

        public bool CanConvertTo(Type t)
        {
            return DictType.IsAssignableFrom(t)
                || (t.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(t.GetGenericTypeDefinition()));
        }

        public bool CanConvertToToml() => false;

        public object Convert(ITomlRoot root, object value, Type targetType)
        {
            var table = (TomlTable)value;
            var ctx = new TomlSettings.CreateInstanceContext((string)null);
            var converted = (IDictionary)table.Root.Settings.GetActivatedInstance(targetType, ctx);
            var elementType = DictReflectUtil.GetElementType(converted.GetType());

            foreach (var r in table.Rows)
            {
                converted[r.Key] = r.Value.GetInternal(elementType, r.Key.ToEnumerable);
            }

            return converted;
        }
    }
}
