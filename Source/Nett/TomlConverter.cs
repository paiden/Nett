namespace Nett
{
    using System;

    internal sealed class TomlConverter<TFrom, TTo> : TomlConverterBase<TFrom, TTo>
    {
        private readonly Func<ITomlRoot, TFrom, TTo> convert;

        public TomlConverter(Func<ITomlRoot, TFrom, TTo> convert)
        {
            if (convert == null) { throw new ArgumentNullException(nameof(convert)); }

            this.convert = convert;
        }

        public override TTo Convert(ITomlRoot root, TFrom from, Type targetType) => this.convert(root, from);
    }
}
