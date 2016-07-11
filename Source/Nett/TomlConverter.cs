namespace Nett
{
    using System;

    internal sealed class TomlConverter<TFrom, TTo> : TomlConverterBase<TFrom, TTo>
    {
        private readonly Func<IMetaDataStore, TFrom, TTo> convert;

        public TomlConverter(Func<IMetaDataStore, TFrom, TTo> convert)
        {
            if (convert == null) { throw new ArgumentNullException(nameof(convert)); }

            this.convert = convert;
        }

        public override TTo Convert(IMetaDataStore metaData, TFrom from, Type targetType) => this.convert(metaData, from);
    }
}
