using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nett
{
    [DebuggerDisplay("{FromType} -> {ToType}")]
    internal sealed class TomlConverter<TFrom, TTo> : TomlConverterBase<TFrom, TTo>
    {
        private readonly Func<TFrom, TTo> convert;

        public TomlConverter(Func<TFrom, TTo> convert)
        {
            if(convert == null) { throw new ArgumentNullException(nameof(convert)); }

            this.convert = convert;
        }

        public override TTo Convert(TFrom from) => this.convert(from);
    }
}
