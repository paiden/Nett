namespace Nett
{
    using System;
    using System.Linq;

    internal abstract class TomlConverterBase<TFrom, TTo> : ITomlConverter<TFrom, TTo>
    {
        public static readonly Type StaticFromType = typeof(TFrom);
        public static readonly Type StaticToType = typeof(TTo);
        private static readonly bool CanConvertToTomlType = Types.TomlObjectType.IsAssignableFrom(typeof(TTo));
        private static readonly TomlObjectType? StaticTomlTargetType = GetTomlTargetType(typeof(TTo));

        public TomlObjectType? TomlTargetType => StaticTomlTargetType;

        public Type FromType => StaticFromType;

        public bool CanConvertFrom(Type t) => t == StaticFromType;

        public bool CanConvertTo(Type t) => t == StaticToType || t.IsAssignableFrom(StaticToType);

        public bool CanConvertToToml() => CanConvertToTomlType;

        public object Convert(ITomlRoot root, object o, Type targetType) => this.Convert(root, (TFrom)o, targetType);

        public abstract TTo Convert(ITomlRoot root, TFrom from, Type targetType);

        private static TomlObjectType? GetTomlTargetType(Type t)
        {
            foreach (var ev in Enum.GetValues(typeof(TomlObjectType))
                .Cast<TomlObjectType>())
            {
                if (t == ev.ToTomlClassType()) { return ev; }
            }

            return null;
        }
    }
}
