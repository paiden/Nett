using System;
using System.Collections.Generic;
using System.Linq;
using Nett.LinqExtensions;

using static System.Diagnostics.Debug;

namespace Nett.Converters
{
    internal sealed class TomlArrayToObjectConverter : ITomlConverter<TomlArray, object>
    {
        public Type FromType => typeof(TomlArray);

        public TomlObjectType? TomlTargetType => TomlObjectType.Array;

        public bool CanConvertFrom(Type t) => t == this.FromType;

        public bool CanConvertTo(Type t) => typeof(object).IsAssignableFrom(t);

        public bool CanConvertToToml() => false;

        public TomlArray Convert(ITomlRoot root, object src, Type targetType)
        {
            throw new NotImplementedException();
        }

        public object Convert(ITomlRoot root, TomlArray src, Type targetType)
        {
            Assert(targetType == typeof(object), "Check converter registration order.");
            var listItems = src.Items.Select(i => i.Get<object>());
            return new List<object>(listItems);
        }

        object ITomlConverter.Convert(ITomlRoot root, object value, Type targetType)
            => this.Convert(root, (TomlArray)value, targetType);
    }
}
