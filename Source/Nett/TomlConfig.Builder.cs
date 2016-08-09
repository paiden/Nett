namespace Nett
{
    using System;
    using System.Collections.Generic;
    using static System.Diagnostics.Debug;

    public sealed partial class TomlConfig
    {
        private static readonly List<ITomlConverter> CastingConverters = new List<ITomlConverter>()
        {
            // TOML -> CLR
            // TomlFloat -> *
            new TomlConverter<TomlFloat, long>((m, f) => (long)f.Value),
            new TomlConverter<TomlFloat, ulong>((m, f) => (ulong)f.Value),
            new TomlConverter<TomlFloat, int>((m, f) => (int)f.Value),
            new TomlConverter<TomlFloat, uint>((m, f) => (uint)f.Value),
            new TomlConverter<TomlFloat, short>((m, f) => (short)f.Value),
            new TomlConverter<TomlFloat, ushort>((m, f) => (ushort)f.Value),
            new TomlConverter<TomlFloat, char>((m, f) => (char)f.Value),
            new TomlConverter<TomlFloat, byte>((m, f) => (byte)f.Value),
            new TomlConverter<TomlFloat, float>((m, f) => (float)f.Value),

            // TomlInt -> *
            new TomlConverter<TomlInt, ulong>((m, i) => (ulong)i.Value),
            new TomlConverter<TomlInt, int>((m, i) => (int)i.Value),
            new TomlConverter<TomlInt, uint>((m, i) => (uint)i.Value),
            new TomlConverter<TomlInt, short>((m, i) => (short)i.Value),
            new TomlConverter<TomlInt, ushort>((m, i) => (ushort)i.Value),
            new TomlConverter<TomlInt, char>((m, i) => (char)i.Value),
            new TomlConverter<TomlInt, byte>((m, i) => (byte)i.Value),
            new TomlConverter<TomlInt, float>((m, i) => i.Value),
            new TomlConverter<TomlInt, double>((m, i) => i.Value),

            // CLR -> TOML
            // * -> TomlInt
            new TomlConverter<ulong, TomlInt>((m, v) => new TomlInt(m, (long)v)),
            new TomlConverter<int, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<short, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<ushort, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<char, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<byte, TomlInt>((m, v) => new TomlInt(m, v)),

            // * -> TomlFloat
            new TomlConverter<float, TomlFloat>((m, v) => new TomlFloat(m, v)),
        }
            .AddBidirectionalConverter<TomlInt, TomlFloat>((m, f) => new TomlInt(m, (long)f.Value), (m, i) => new TomlFloat(m, i.Value))
            .AddBidirectionalConverter<TomlDateTime, DateTime>((m, c) => new TomlDateTime(m, c), (m, t) => t.Value.UtcDateTime);

        private static readonly List<ITomlConverter> EquivalentConverters = new List<ITomlConverter>()
            .AddBidirectionalConverter<TomlInt, long>((m, c) => new TomlInt(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlFloat, double>((m, c) => new TomlFloat(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlString, string>((m, c) => new TomlString(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlDateTime, DateTimeOffset>((m, c) => new TomlDateTime(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlTimeSpan, TimeSpan>((m, c) => new TomlTimeSpan(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlBool, bool>((m, c) => new TomlBool(m, c), (m, t) => t.Value);

        private static readonly List<ITomlConverter> ParseConverters = new List<ITomlConverter>()
        {
            // TomlStrings <-> enums
            new TomlToEnumConverter(),
            new EnumToTomlConverter(),

            // Dict <-> TomlTable
            new TomlTableToDictionaryConverter(),
            new TomlTableToTypedDictionaryConverter(),
        }
        .AddBidirectionalConverter<TomlString, Guid>((m, c) => new TomlString(m, c.ToString("D")), (m, t) => Guid.Parse(t.Value));

        public enum ConversionLevel
        {
            Strict = ConversionSets.Equivalent,
            Cast = Strict | ConversionSets.Cast,
            Convert = Cast | ConversionSets.Convert,
        }

        [Flags]
        public enum ConversionSets
        {
            Equivalent = 1 << 0,
            Cast = 1 << 1,
            Convert = 1 << 2,
        }

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

            ITomlConfigBuilder AllowImplicitConversions(ConversionLevel level);

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
                const ConversionLevel DefaultConversionSettings = ConversionLevel.Convert;
                this.AllowImplicitConversions(DefaultConversionSettings);
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

                this.config.converters.AddRange(EquivalentConverters);

                if (this.allowedConversions.HasFlag(ConversionSets.Cast))
                {
                    this.config.converters.AddRange(CastingConverters);
                }

                if (this.allowedConversions.HasFlag(ConversionSets.Convert))
                {
                    this.config.converters.AddRange(ParseConverters);
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

    internal static class CreateBindingExtensions
    {
        public static List<ITomlConverter> AddBidirectionalConverter<TToml, TClr>(
            this List<ITomlConverter> converterlist,
            Func<IMetaDataStore, TClr, TToml> toToml,
            Func<IMetaDataStore, TToml, TClr> toClr)
            where TToml : TomlObject
        {
            converterlist.Add(new TomlConverter<TToml, TClr>(toClr));
            converterlist.Add(new TomlConverter<TClr, TToml>(toToml));
            return converterlist;
        }
    }
}
