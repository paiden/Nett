using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public abstract class TomlValue : TomlObject
    {

    }

    public sealed class TomlValue<T> : TomlValue
    {
        private readonly T value;
        public T Value => this.value;

        public TomlValue(T value)
        {
            this.value = value;
        }

        public override TRes Get<TRes>()
        {
            return Converter.Convert<TRes>(this.Value);
        }

        public override object Get(Type t)
        {
            return Converter.Convert(t, this.Value);
        }
    }
}
