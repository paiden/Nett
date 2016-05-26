using System;

namespace Nett
{
    internal interface ITomlConverter
    {
        bool CanConvertFrom(Type t);
        bool CanConvertTo(Type t);
        bool CanConvertToToml();

        Type FromType { get; }
        object Convert(IMetaDataStore metaData, object value, Type targetType);
    }

    internal interface ITomlConverter<TFrom, TTo> : ITomlConverter
    {
        TTo Convert(IMetaDataStore metaData, TFrom src, Type targetType);
    }
}
