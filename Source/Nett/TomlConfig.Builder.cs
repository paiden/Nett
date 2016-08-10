namespace Nett
{
    using System;
    using System.Collections.Generic;
    using static System.Diagnostics.Debug;

    public sealed partial class TomlConfig
    {
        public interface IConfigureTypeBuilder<TCustom>
        {
            IConfigureTypeBuilder<TCustom> CreateInstance(Func<TCustom> func);

            IConfigureTypeBuilder<TCustom> TreatAsInlineTable();

            IConfigureTypeBuilder<TCustom> WithConversionFor<TToml>(Action<IConfigureConversionBuilder<TCustom, TToml>> conv)
                where TToml : TomlObject;
        }

        public interface ITableKeyMappingBuilder
        {
            ITomlConfigBuilder To<T>();
        }

        public interface ITomlConfigBuilder
        {
            ITomlConfigBuilder AllowImplicitConversions(ConversionSets sets);

            ITomlConfigBuilder Apply(Action<ITomlConfigBuilder> batch);

            ITomlConfigBuilder ConfigureType<T>(Action<IConfigureTypeBuilder<T>> ct);

            ITableKeyMappingBuilder MapTableKey(string key);
        }

        internal sealed class ConversionConfigurationBuilder<TCustom, TToml> : IConfigureConversionBuilder<TCustom, TToml>
            where TToml : TomlObject
        {
            private readonly List<ITomlConverter> converters;

            public ConversionConfigurationBuilder(List<ITomlConverter> converters)
            {
                Assert(converters != null);

                this.converters = converters;
            }

            public IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<IMetaDataStore, TToml, TCustom> convert)
            {
                this.AddConverter(new TomlConverter<TToml, TCustom>(convert));
                return this;
            }

            public IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<TToml, TCustom> convert)
            {
                this.AddConverterInternal(new TomlConverter<TToml, TCustom>((_, tToml) => convert(tToml)));
                return this;
            }

            public IConfigureConversionBuilder<TCustom, TToml> ToToml(Func<IMetaDataStore, TCustom, TToml> convert)
            {
                this.AddConverterInternal(new TomlConverter<TCustom, TToml>(convert));
                return this;
            }

            internal void AddConverter(ITomlConverter converter) => this.converters.Add(converter);

            private void AddConverterInternal(ITomlConverter converter)
            {
                this.converters.Insert(0, converter);
            }
        }

        internal sealed class TableKeyMappingBuilder : ITableKeyMappingBuilder
        {
            private readonly TomlConfig config;
            private readonly ITomlConfigBuilder configBuilder;
            private readonly string key;

            public TableKeyMappingBuilder(TomlConfig config, ITomlConfigBuilder configBuilder, string key)
            {
                this.config = config;
                this.configBuilder = configBuilder;
                this.key = key;
            }

            public ITomlConfigBuilder To<T>()
            {
                this.config.tableKeyToTypeMappings[this.key] = typeof(T);
                return this.configBuilder;
            }
        }

        internal sealed class TomlConfigBuilder : ITomlConfigBuilder
        {
            private readonly TomlConfig config = new TomlConfig();
            private readonly List<ITomlConverter> userConverters = new List<ITomlConverter>();

            private ConversionSets allowedConversions;

            public TomlConfigBuilder(TomlConfig config)
            {
                Assert(config != null);

                this.config = config;
                this.AllowImplicitConversions(ConversionSets.All);
            }

            public ITomlConfigBuilder AllowImplicitConversions(ConversionSets sets)
            {
                this.allowedConversions = sets;
                return this;
            }

            public ITomlConfigBuilder Apply(Action<ITomlConfigBuilder> batch)
            {
                batch(this);
                return this;
            }

            public ITomlConfigBuilder ConfigureType<T>(Action<IConfigureTypeBuilder<T>> ct)
            {
                ct(new TypeConfigurationBuilder<T>(this.config, this.userConverters));
                return this;
            }

            public ITableKeyMappingBuilder MapTableKey(string key) =>
                new TableKeyMappingBuilder(this.config, this, key);

            public void SetupConverters()
            {
                this.SetupDefaultConverters();
                this.SetupUserConverters();
            }

            public void SetupDefaultConverters()
            {
                Assert(this.allowedConversions != 0);

                this.config.converters.AddRange(StrictConverters);

                if (this.allowedConversions.HasFlag(ConversionSets.Cast))
                {
                    this.config.converters.AddRange(CastConverters);
                }

                if (this.allowedConversions.HasFlag(ConversionSets.Convert))
                {
                    this.config.converters.AddRange(ConvertConverters);
                }
            }

            private void SetupUserConverters()
            {
                this.config.converters.AddRange(this.userConverters);
            }
        }

        internal sealed class TypeConfigurationBuilder<TCustom> : IConfigureTypeBuilder<TCustom>
        {
            private readonly TomlConfig config;
            private readonly List<ITomlConverter> converters;

            public TypeConfigurationBuilder(TomlConfig config, List<ITomlConverter> converters)
            {
                Assert(config != null);
                Assert(converters != null);

                this.config = config;
                this.converters = converters;
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

            public IConfigureTypeBuilder<TCustom> WithConversionFor<TToml>(Action<IConfigureConversionBuilder<TCustom, TToml>> conv)
                where TToml : TomlObject
            {
                conv(new ConversionConfigurationBuilder<TCustom, TToml>(this.converters));
                return this;
            }
        }
    }
}
