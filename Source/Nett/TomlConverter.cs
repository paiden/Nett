using System;

namespace Nett
{
    internal sealed class TomlConverter<TFrom, TTo> : TomlConverterBase<TFrom, TTo>
    {
        private readonly Func<TFrom, TTo> convert;

        public TomlConverter(Func<TFrom, TTo> convert)
        {
            if (convert == null) { throw new ArgumentNullException(nameof(convert)); }

            this.convert = convert;
        }

        public override TTo Convert(TFrom from, Type targetType) => this.convert(from);
    }
}
