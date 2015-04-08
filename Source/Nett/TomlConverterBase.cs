using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public abstract class TomlConverterBase<TTarget, TSource> : ITomlConverter<TTarget, TSource>
    {
        private static readonly Type TargetTypeInternal = typeof(TTarget);
        private static readonly Type SourceTypeInternal = typeof(TSource);
        public Type SourceType => SourceTypeInternal;
        public Type TargetType => TargetTypeInternal;

        public abstract TTarget FromToml(TSource src);

        public abstract TomlObject ToToml(TTarget value);

        public TomlObject ToToml(object value)
        {
            return this.ToToml((TTarget)value);
        }
        public object FromToml(object value)
        {
            return this.FromToml((TSource)value);
        }
    }
}
