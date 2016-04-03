using System;
using System.Diagnostics;

namespace Nett
{
    public sealed partial class TomlConfig
    {
        //public IFrom AddConversion() => new From(this);

        //private class From : IFrom
        //{
        //    private readonly TomlConfig config;
        //    public From(TomlConfig config)
        //    {
        //        Debug.Assert(config != null);
        //        this.config = config;
        //    }

        //    public Type FromType { get; }

        //    ITo<T> IFrom.From<T>() => new To<T>(this.config);
        //}

        //private sealed class To<TFrom> : ITo<TFrom>
        //{
        //    private readonly TomlConfig config;

        //    public To(TomlConfig config)
        //    {
        //        Debug.Assert(config != null);
        //        this.config = config;
        //    }
        //    IAs<TFrom, T> ITo<TFrom>.To<T>() => new As<TFrom, T>(this.config);
        //}

        //internal sealed class As<TFrom, TTo> : IAs<TFrom, TTo>
        //{
        //    private readonly TomlConfig config;

        //    public As(TomlConfig config)
        //    {
        //        Debug.Assert(config != null);
        //        this.config = config;
        //    }

        //    TomlConfig IAs<TFrom, TTo>.As(Func<TFrom, TTo> convert)
        //    {
        //        var conv = new TomlConverter<TFrom, TTo>(convert);
        //        this.config.AddConverter(conv);
        //        return this.config;
        //    }
        //}

        public IConfigureTypeStart<T> ConfigureType<T>() => new TypeConfigurator<T>(this);

        public interface IConfigureTypeStart<T>
        {
            IConfigureType<T> As { get; }
        }

        public interface IConfigureTypeCombiner<T>
        {
            IConfigureType<T> And { get; }

            TomlConfig Apply();
        }

        public interface IConfigureType<T>
        {
            IConfigureTypeCombiner<T> CreateWith(Func<T> activator);

            IConfigureCast<T, TFrom, T> ConvertFrom<TFrom>() where TFrom : TomlObject;
            IConfigureCast<T, T, TTo> ConvertTo<TTo>() where TTo : TomlObject;

            IConfigureTypeCombiner<T> TreatAsInlineTable();
        }

        public interface IConfigureCast<T, TFrom, TTo>
        {
            IConfigureTypeCombiner<T> As(Func<TFrom, TTo> cast);
        }

        public class TypeConfigurator<T> : IConfigureType<T>, IConfigureTypeCombiner<T>, IConfigureTypeStart<T>
        {
            private readonly TomlConfig config;

            public TypeConfigurator(TomlConfig config)
            {
                Debug.Assert(config != null);

                this.config = config;
            }

            public IConfigureType<T> And => this;

            public IConfigureType<T> As => this;

            public IConfigureTypeCombiner<T> CreateWith(Func<T> activator)
            {
                if (activator == null) { throw new ArgumentNullException("activator"); }
                this.config.activators.Add(typeof(T), () => activator());
                return this;
            }

            public IConfigureCast<T, TFrom, T> ConvertFrom<TFrom>() where TFrom : TomlObject => new FromTomlCastConfigurator<T, TFrom, T>(this.config, this);

            public IConfigureCast<T, T, TTo> ConvertTo<TTo>() where TTo : TomlObject => new ToTomlCastConfigurator<T, T, TTo>(this.config, this);

            public IConfigureTypeCombiner<T> TreatAsInlineTable()
            {
                config.inlineTableTypes.Add(typeof(T));
                return this;
            }

            public TomlConfig Apply() => this.config;
        }

        private abstract class CastConfigurator<T>
        {
            protected readonly TomlConfig config;
            protected readonly TypeConfigurator<T> typeConfig;

            public CastConfigurator(TomlConfig config, TypeConfigurator<T> typeConfig)
            {
                Debug.Assert(config != null);
                Debug.Assert(typeConfig != null);

                this.config = config;
                this.typeConfig = typeConfig;
            }
        }

        private class ToTomlCastConfigurator<T, TFrom, TTo> : CastConfigurator<T>, IConfigureCast<T, TFrom, TTo>
        {
            public ToTomlCastConfigurator(TomlConfig config, TypeConfigurator<T> typeConfig) :
                base(config, typeConfig)
            {
            }

            public IConfigureTypeCombiner<T> As(Func<TFrom, TTo> cast)
            {
                var conv = new TomlConverter<TFrom, TTo>(cast);
                config.AddConverter(conv);
                return this.typeConfig;
            }
        }

        private class FromTomlCastConfigurator<T, TFrom, TTo> : CastConfigurator<T>, IConfigureCast<T, TFrom, TTo>
        {
            public FromTomlCastConfigurator(TomlConfig config, TypeConfigurator<T> typeConfig)
                : base(config, typeConfig)
            {
            }

            public IConfigureTypeCombiner<T> As(Func<TFrom, TTo> cast)
            {
                var conv = new TomlConverter<TFrom, TTo>(cast);
                config.AddConverter(conv);
                return this.typeConfig;
            }
        }
    }
}
