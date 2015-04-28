using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public abstract class TomlConverterBase<TFrom, TTo> : ITomlConverter<TFrom, TTo>
    {
        public static readonly Type StaticFromType = typeof(TFrom);
        public static readonly Type StaticToType = typeof(TTo);

        public Type FromType => StaticFromType;
        public Type ToType => StaticToType;

        public object Convert(object o) => (TTo)this.Convert((TFrom)o);
        public abstract TTo Convert(TFrom from);
    }
}
