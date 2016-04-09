using System;

namespace Nett
{
    internal interface ITomlConverter
    {
        bool CanConvertFrom(Type t);
        bool CanConvertTo(Type t);
        bool CanConvertToToml();

        Type FromType { get; }
        object Convert(object value, Type targetType);
    }

    internal interface ITomlConverter<TFrom, TTo> : ITomlConverter
    {
        TTo Convert(TFrom src, Type targetType);
    }
}
