using System;
using static System.Diagnostics.Debug;

namespace Nett
{
    public sealed partial class TomlConfig
    {
        public interface ITomlConfigBuilder
        {
            ITomlConfigBuilder ConfigureType<T>(Action<IConfigureTypeBuilder<T>> ct);
            ITomlConfigBuilder Apply(Action<ITomlConfigBuilder> batch);
        }

        public interface IConfigureTypeBuilder<TCustom>
        {
            IConfigureTypeBuilder<TCustom> WithConversionFor<TToml>(Action<IConfigureConversionBuilder<TCustom, TToml>> conv) where TToml : TomlObject;
            IConfigureTypeBuilder<TCustom> CreateInstance(Func<TCustom> func);
            IConfigureTypeBuilder<TCustom> TreatAsInlineTable();
        }

        public interface IConfigureConversionBuilder<TCustom, TToml> where TToml : TomlObject
        {
            IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<TToml, TCustom> convert);
            IConfigureConversionBuilder<TCustom, TToml> ToToml(Func<TCustom, TToml> convert);
        }

        public IConfigureTypeBuilder<TCustom> ConfigureType<TCustom>() => new TypeConfigurationBuilder<TCustom>(this);

        internal sealed class TomlConfigBuilder : ITomlConfigBuilder
        {
            private readonly TomlConfig config = new TomlConfig();

            public TomlConfigBuilder(TomlConfig config)
            {
                Assert(config != null);

                this.config = config;
            }

            public ITomlConfigBuilder Apply(Action<ITomlConfigBuilder> batch)
            {
                batch(this);
                return this;
            }

            public ITomlConfigBuilder ConfigureType<T>(Action<IConfigureTypeBuilder<T>> ct)
            {
                ct(new TypeConfigurationBuilder<T>(this.config));
                return this;
            }

        }

        internal sealed class TypeConfigurationBuilder<TCustom> : IConfigureTypeBuilder<TCustom>
        {
            private readonly TomlConfig config;

            public TypeConfigurationBuilder(TomlConfig config)
            {
                Assert(config != null);

                this.config = config;
            }

            public IConfigureTypeBuilder<TCustom> CreateInstance(Func<TCustom> activator)
            {
                this.config.activators.Add(typeof(TCustom), () => activator());
                return this;
            }

            public IConfigureTypeBuilder<TCustom> TreatAsInlineTable()
            {
                this.config.inlineTableTypes.Add(typeof(TCustom));
                return this;
            }

            public IConfigureTypeBuilder<TCustom> WithConversionFor<TToml>(Action<IConfigureConversionBuilder<TCustom, TToml>> conv) where TToml : TomlObject
            {
                conv(new ConversionConfigurationBuilder<TCustom, TToml>(this.config));
                return this;
            }
        }

        internal sealed class ConversionConfigurationBuilder<TCustom, TToml> : IConfigureConversionBuilder<TCustom, TToml> where TToml : TomlObject
        {
            private readonly TomlConfig config;

            public ConversionConfigurationBuilder(TomlConfig config)
            {
                Assert(config != null);

                this.config = config;
            }

            public IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<TToml, TCustom> convert)
            {
                this.config.AddConverter(new TomlConverter<TToml, TCustom>(convert));
                return this;
            }

            public IConfigureConversionBuilder<TCustom, TToml> ToToml(Func<TCustom, TToml> convert)
            {
                this.config.AddConverter(new TomlConverter<TCustom, TToml>(convert));
                return this;
            }
        }
    }
}
