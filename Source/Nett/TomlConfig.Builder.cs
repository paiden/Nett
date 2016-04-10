using System;
using System.Collections.Generic;
using static System.Diagnostics.Debug;

namespace Nett
{
    public sealed partial class TomlConfig
    {
        private static readonly List<ITomlConverter> EquivalentConverters = new List<ITomlConverter>()
        {
            new TomlConverter<TomlInt, long>(t => (long)t.Value),
            new TomlConverter<TomlFloat, double>(t => t.Value),
            new TomlConverter<TomlString, string>(t => t.Value),
            new TomlConverter<TomlDateTime, DateTimeOffset>(t => t.Value),
            new TomlConverter<TomlTimeSpan, TimeSpan>(t => t.Value),
            new TomlConverter<TomlBool, bool>(t => t.Value)
        };

        private static readonly List<ITomlConverter> SameNumericalTypeConverters = new List<ITomlConverter>()
        {
            // TomlInt to integer types
            new TomlConverter<TomlInt, char>(t => (char)t.Value),
            new TomlConverter<TomlInt, byte>(t => (byte)t.Value),
            new TomlConverter<TomlInt, int>(t => (int)t.Value),
            new TomlConverter<TomlInt, short>(t => (short)t.Value),

            // TomlFloat to floating point types
            new TomlConverter<TomlFloat, float>(t => (float)t.Value),

            // TomlDateTime to 'simpler' datetime
            new TomlConverter<TomlDateTime, DateTime>(t => t.Value.UtcDateTime),

            // TomlStrings <-> enums
            new TomlToEnumConverter(),
            new EnumToTomlConverter(),
        };

        private static readonly List<ITomlConverter> DotNetImplicitConverters = new List<ITomlConverter>()
        {
            // Int to float
            new TomlConverter<TomlInt, float>(i => i.Value),
            new TomlConverter<TomlInt, double>(i => i.Value),
            new TomlConverter<TomlInt, TomlFloat>(i => new TomlFloat(i.Value)),
        };

        private static readonly List<ITomlConverter> DotNetExplicitConverters = new List<ITomlConverter>()
        {
            // TomlFloat to *
            new TomlConverter<TomlFloat, TomlInt>(f => new TomlInt((int)f.Value)),
            new TomlConverter<TomlFloat, long>(f => (long)f.Value),
            new TomlConverter<TomlFloat, int>(f => (int)f.Value),
            new TomlConverter<TomlFloat, short>(f => (short)f.Value),
            new TomlConverter<TomlFloat, char>(f => (char)f.Value),

            // TomlInt to *
        };

        [Flags]
        public enum ConversionSets
        {
            Equivalent = 1 << 0,
            SameNumericCategory = 1 << 1,
            DotNetImplicit = 1 << 2,
            DotNetExplicit = 1 << 3,
            Parse = 1 << 4,
        }

        public enum ConversionLevel
        {
            Strict = ConversionSets.Equivalent,
            SameNumericCategory = Strict | ConversionSets.SameNumericCategory,
            DotNetImplicit = SameNumericCategory | ConversionSets.DotNetImplicit,
            DotNetExplicit = DotNetImplicit | ConversionSets.DotNetExplicit,
            Parse = DotNetExplicit | ConversionSets.Parse,
        }

        public interface ITomlConfigBuilder
        {
            ITomlConfigBuilder ConfigureType<T>(Action<IConfigureTypeBuilder<T>> ct);
            ITomlConfigBuilder Apply(Action<ITomlConfigBuilder> batch);
            ITomlConfigBuilder AllowImplicitConversions(ConversionSets sets);
            ITomlConfigBuilder AllowImplicitConversions(ConversionLevel level);
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

            private ConversionSets allowedConversions;

            public TomlConfigBuilder(TomlConfig config)
            {
                Assert(config != null);

                this.config = config;
                const ConversionLevel DefaultConversionSettings = ConversionLevel.SameNumericCategory;
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
                if (this.allowedConversions.HasFlag(ConversionSets.SameNumericCategory))
                {
                    this.config.converters.AddRange(SameNumericalTypeConverters);
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
