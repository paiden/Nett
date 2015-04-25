using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed partial class TomlConfig
    {
        public IFrom AddConversion()
        {
            return new From(this);
        }

        private class From : IFrom
        {
            private readonly TomlConfig config;
            public From(TomlConfig config)
            {
                Debug.Assert(config != null);
                this.config = config;
            }

            public Type FromType { get; private set; }

            ITo<T> IFrom.From<T>()
            {
                return new To<T>(this.config);
            }
        }

        private sealed class To<TFrom> : ITo<TFrom>
        {
            private readonly TomlConfig config;

            public To(TomlConfig config)
            {
                Debug.Assert(config != null);
                this.config = config;
            }
            IAs<TFrom, T> ITo<TFrom>.To<T>()
            {
                return new As<TFrom, T>(this.config);
            }
        }

        internal sealed class As<TFrom, TTo> : IAs<TFrom, TTo>
        {
            private readonly TomlConfig config;

            public As(TomlConfig config)
            {
                Debug.Assert(config != null);
                this.config = config;
            }

            TomlConfig IAs<TFrom, TTo>.As(Func<TFrom, TTo> convert)
            {
                var conv = new TomlConverter<TFrom, TTo>(convert);
                this.config.AddConverter(conv);
                return this.config;
            }
        }
    }

    public interface IFrom
    {
        ITo<T> From<T>();
    }

    public interface ITo<TFrom>
    {
        IAs<TFrom, T> To<T>();
    }

    public interface IAs<TFrom, TTo>
    {
        TomlConfig As(Func<TFrom, TTo> convert);
    }
}
