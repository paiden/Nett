namespace Nett
{
    using System;

    internal interface ITomlConverter
    {
        Type FromType { get; }

        bool CanConvertFrom(Type t);

        bool CanConvertTo(Type t);

        bool CanConvertToToml();

        object Convert(IMetaDataStore metaData, object value, Type targetType);
    }

    internal interface ITomlConverter<TFrom, TTo> : ITomlConverter
    {
        TTo Convert(IMetaDataStore metaData, TFrom src, Type targetType);
    }
}
