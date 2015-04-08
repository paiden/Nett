using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlConverter<TTarget, TSource> : TomlConverterBase<TTarget, TSource>, INeedsToTomlConverter<TTarget, TSource>
    {
        private readonly Func<TSource, TTarget> fromToml;
        private readonly Func<TTarget, TomlObject> toToml;

        private TomlConverter(Func<TSource, TTarget> fromToml, Func<TTarget, TomlObject> toToml)
        {
            this.fromToml = fromToml;
            this.toToml = toToml;
        }

        public static INeedsToTomlConverter<TTarget, TSource> FromlToml(Func<TSource, TTarget> from)
        {
            return new TomlConverter<TTarget, TSource>(from, null);
        }

        public override TTarget FromToml(TSource src)
        {
            return this.fromToml(src);
        }

        public override TomlObject ToToml(TTarget value)
        {
            return this.toToml(value);
        }

        public TomlConverter<TTarget, TSource> ToToml(Func<TTarget, TomlObject> convert)
        {
            if (convert == null) { throw new ArgumentNullException(nameof(convert)); }

            return new TomlConverter<TTarget, TSource>(this.fromToml, convert);
        }
    }

    public interface INeedsToTomlConverter<TTarget, TSource>
    {
        TomlConverter<TTarget, TSource> ToToml(Func<TTarget, TomlObject> convert);
    }
}
