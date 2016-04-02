using System;
using System.Diagnostics;

namespace Nett
{
    public sealed partial class TomlConfig
    {
        public interface IConfigureType<TCustom>
        {
            IConfigureConversion<TCustom, TToml> WithConversionFor<TToml>() where TToml : TomlObject;
            IConfigureType<TCustom> CreateInstanceAs(Func<TCustom> func);
            TomlConfig Configure();
            IConfigureType<TCustom> TreatAsInlineTable();
        }

        public interface IConfigureConversion<TCustom, TToml> where TToml : TomlObject
        {
            IConfigureConversion<TCustom, TToml> ConvertFromAs(Func<TToml, TCustom> convert);
            IConfigureConversion<TCustom, TToml> ConvertToAs(Func<TCustom, TToml> convert);
            IConfigureType<TCustom> Apply();
            TomlConfig Configure();
        }

        public IConfigureType<TCustom> ConfigureType<TCustom>() => new TypeConfigurationBuilder<TCustom>(this);

        internal sealed class TypeConfigurationBuilder<TCustom> : IConfigureType<TCustom>
        {
            private readonly TomlConfig config;

            public TypeConfigurationBuilder(TomlConfig config)
            {
                Debug.Assert(config != null);

                this.config = config;
            }

            public TomlConfig Apply() => this.config;

            public TomlConfig Configure() => this.config;

            public IConfigureType<TCustom> CreateInstanceAs(Func<TCustom> activator)
            {
                this.config.activators.Add(typeof(TCustom), () => activator());
                return this;
            }

            public IConfigureType<TCustom> TreatAsInlineTable()
            {
                this.config.inlineTableTypes.Add(typeof(TCustom));
                return this;
            }

            public IConfigureConversion<TCustom, TToml> WithConversionFor<TToml>() where TToml : TomlObject =>
                new ConversionConfigurationBuilder<TCustom, TToml>(this.config, this);
        }

        internal sealed class ConversionConfigurationBuilder<TCustom, TToml> : IConfigureConversion<TCustom, TToml> where TToml : TomlObject
        {
            private readonly TomlConfig config;
            private readonly TypeConfigurationBuilder<TCustom> typeConfig;

            public ConversionConfigurationBuilder(TomlConfig config, TypeConfigurationBuilder<TCustom> typeConfig)
            {
                Debug.Assert(config != null);
                Debug.Assert(typeConfig != null);

                this.config = config;
                this.typeConfig = typeConfig;
            }

            public IConfigureType<TCustom> Apply() => this.typeConfig;

            public TomlConfig Configure() => this.config;

            public IConfigureConversion<TCustom, TToml> ConvertFromAs(Func<TToml, TCustom> convert)
            {
                this.config.AddConverter(new TomlConverter<TToml, TCustom>(convert));
                return this;
            }

            public IConfigureConversion<TCustom, TToml> ConvertToAs(Func<TCustom, TToml> convert)
            {
                this.config.AddConverter(new TomlConverter<TCustom, TToml>(convert));
                return this;
            }
        }
    }
}
