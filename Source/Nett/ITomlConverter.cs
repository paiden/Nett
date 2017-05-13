namespace Nett
{
    using System;

    internal interface ITomlConverter
    {
        Type FromType { get; }

        TomlObjectType? TomlTargetType { get; }

        bool CanConvertFrom(Type t);

        bool CanConvertTo(Type t);

        bool CanConvertToToml();

        object Convert(ITomlRoot root, object value, Type targetType);
    }

    internal interface ITomlConverter<TFrom, TTo> : ITomlConverter
    {
        TTo Convert(ITomlRoot root, TFrom src, Type targetType);
    }
}