using System;
using System.Collections.Generic;
using static System.Diagnostics.Debug;

namespace Nett
{
    public sealed partial class TomlConfig
    {
        private static readonly List<ITomlConverter> EquivalentConverters = new List<ITomlConverter>()
        {
            new TomlConverter<TomlInt, long>((m, t) => (long)t.Value),
            new TomlConverter<TomlFloat, double>((m, t) => t.Value),
            new TomlConverter<TomlString, string>((m, t) => t.Value),
            new TomlConverter<TomlDateTime, DateTimeOffset>((m, t) => t.Value),
            new TomlConverter<TomlTimeSpan, TimeSpan>((m, t) => t.Value),
            new TomlConverter<TomlBool, bool>((m, t) => t.Value)
        };

        private static readonly List<ITomlConverter> MatchingConverters = new List<ITomlConverter>()
        {
            // TomlInt to integer types
            new TomlConverter<TomlInt, char>((m, t) => (char)t.Value),
            new TomlConverter<TomlInt, byte>((m, t) => (byte)t.Value),
            new TomlConverter<TomlInt, int>((m, t) => (int)t.Value),
            new TomlConverter<TomlInt, short>((m, t) => (short)t.Value),

            // TomlFloat to floating point types
            new TomlConverter<TomlFloat, float>((m, t) => (float)t.Value),

            // TomlDateTime to 'simpler' datetime
            new TomlConverter<TomlDateTime, DateTime>((m, t) => t.Value.UtcDateTime),

            // TomlStrings <-> enums
            new TomlToEnumConverter(),
            new EnumToTomlConverter(),

            // Dict <-> TomlTable
            new TomlTableToDictionaryConverter(),
        };

        private static readonly List<ITomlConverter> DotNetImplicitConverters = new List<ITomlConverter>()
        {
            // Int to float
            new TomlConverter<TomlInt, float>((m, i) => i.Value),
            new TomlConverter<TomlInt, double>((m, i) => i.Value),
            new TomlConverter<TomlInt, TomlFloat>((m, i) => new TomlFloat(m, i.Value)),
        };

        private static readonly List<ITomlConverter> DotNetExplicitConverters = new List<ITomlConverter>()
        {
            // TomlFloat to *
            new TomlConverter<TomlFloat, TomlInt>((m, f) => new TomlInt(m, (int)f.Value)),
            new TomlConverter<TomlFloat, long>((m, f) => (long)f.Value),
            new TomlConverter<TomlFloat, int>((m, f) => (int)f.Value),
            new TomlConverter<TomlFloat, short>((m, f) => (short)f.Value),
            new TomlConverter<TomlFloat, char>((m, f) => (char)f.Value),

            // TomlInt to *
        };

        [Flags]
        public enum ConversionSets
        {
            Equivalent = 1 << 0,
            Matching = 1 << 1,
            DotNetImplicit = 1 << 2,
            DotNetExplicit = 1 << 3,
            Parse = 1 << 4,
        }

        public enum ConversionLevel
        {
            Strict = ConversionSets.Equivalent,
            Matching = Strict | ConversionSets.Matching,
            DotNetImplicit = Matching | ConversionSets.DotNetImplicit,
            DotNetExplicit = DotNetImplicit | ConversionSets.DotNetExplicit,
            Parse = DotNetExplicit | ConversionSets.Parse,
        }

        public interface ITomlConfigBuilder
        {
            ITomlConfigBuilder ConfigureType<T>(Action<IConfigureTypeBuilder<T>> ct);
            ITomlConfigBuilder Apply(Action<ITomlConfigBuilder> batch);
            ITomlConfigBuilder AllowImplicitConversions(ConversionSets sets);
            ITomlConfigBuilder AllowImplicitConversions(ConversionLevel level);
            ITableKeyMappingBuilder MapTableKey(string key);
        }

        public interface IConfigureTypeBuilder<TCustom>
        {
            IConfigureTypeBuilder<TCustom> WithConversionFor<TToml>(Action<IConfigureConversionBuilder<TCustom, TToml>> conv) where TToml : TomlObject;
            IConfigureTypeBuilder<TCustom> CreateInstance(Func<TCustom> func);
            IConfigureTypeBuilder<TCustom> TreatAsInlineTable();
        }

        public interface ITableKeyMappingBuilder
        {
            ITomlConfigBuilder To<T>();
        }

        public IConfigureTypeBuilder<TCustom> ConfigureType<TCustom>() => new TypeConfigurationBuilder<TCustom>(this);

        internal sealed class TomlConfigBuilder : ITomlConfigBuilder
        {
            private readonly TomlConfig config = new TomlConfig();

            private ConversionSets allowedConversions;

            public TomlConfigBuilder(TomlConfig config)
            {
                Assert(config != null);

                this.config = config;
                const ConversionLevel DefaultConversionSettings = ConversionLevel.Matching;
                this.AllowImplicitConversions(DefaultConversionSettings);
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

            public ITomlConfigBuilder AllowImplicitConversions(ConversionSets sets)
            {
                this.allowedConversions = sets;
                return this;
            }

            public ITomlConfigBuilder AllowImplicitConversions(ConversionLevel level)
            {
                this.allowedConversions = (ConversionSets)level;
                return this;
            }

            public void ApplyConversionSettings()
            {
                Assert(this.allowedConversions != 0);

                if (this.allowedConversions.HasFlag(ConversionSets.Equivalent))
                {
                    this.config.converters.AddRange(EquivalentConverters);
                }
                if (this.allowedConversions.HasFlag(ConversionSets.Matching))
                {
                    this.config.converters.AddRange(MatchingConverters);
                }
                if (this.allowedConversions.HasFlag(ConversionSets.DotNetImplicit))
                {
                    this.config.converters.AddRange(DotNetImplicitConverters);
                }
                if (this.allowedConversions.HasFlag(ConversionSets.DotNetExplicit))
                {
                    this.config.converters.AddRange(DotNetExplicitConverters);
                }
            }

            public ITableKeyMappingBuilder MapTableKey(string key) =>
                new TableKeyMappingBuilder(this.config, this, key);
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

            internal void AddConverter(ITomlConverter converter) => this.config.AddConverter(converter);

            public IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<IMetaDataStore, TToml, TCustom> convert)
            {
                this.config.AddConverter(new TomlConverter<TToml, TCustom>(convert));
                return this;
            }

            public IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<TToml, TCustom> convert)
            {
                this.config.AddConverter(new TomlConverter<TToml, TCustom>((_, tToml) => convert(tToml)));
                return this;
            }

            public IConfigureConversionBuilder<TCustom, TToml> ToToml(Func<IMetaDataStore, TCustom, TToml> convert)
            {
                this.config.AddConverter(new TomlConverter<TCustom, TToml>(convert));
                return this;
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
    }
}
