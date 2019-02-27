using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Nett.TomlSettings;

namespace Nett
{
    public interface IEnableFeatureBuilder
    {
        IEnableFeatureBuilder ValuesWithUnit();
    }

    public static class TomlSettingsBuilderExtensions
    {
        public static ITomlSettingsBuilder EnableExperimentalFeatures(
            this ITomlSettingsBuilder b, Action<IEnableFeatureBuilder> builder)
        {
            var expBuilder = (IExpSettingsBuilder)b;
            var featureBuilder = new EnableFeaturesBuilder(expBuilder);
            builder(featureBuilder);
            return b;
        }

        private class EnableFeaturesBuilder : IEnableFeatureBuilder
        {
            private readonly IExpSettingsBuilder nettBuilder;

            public EnableFeaturesBuilder(IExpSettingsBuilder nettBuilder)
            {
                this.nettBuilder = nettBuilder;
            }

            public IEnableFeatureBuilder ValuesWithUnit()
            {
                this.nettBuilder.EnableExperimentalFeature(ExperimentalFeature.ValueWithUnit, true);
                return this;
            }
        }

        public static IConversionSettingsBuilder<TCustom, TomlBool> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlBool> cb, Func<TCustom, bool> conv, Func<TCustom, string> unit)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlBool>)cb).AddConverter(
                new TomlConverter<TCustom, TomlBool>((root, customValue) =>
                {
                    return new TomlBool(root, conv(customValue))
                    {
                        Unit = unit(customValue),
                    };
                }));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlInt> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlInt> cb, Func<TCustom, long> conv, Func<TCustom, string> unit)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlInt>)cb).AddConverter(
                new TomlConverter<TCustom, TomlInt>((root, customValue) =>
                {
                    return new TomlInt(root, conv(customValue))
                    {
                        Unit = unit(customValue),
                    };
                }));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlFloat> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlFloat> cb, Func<TCustom, double> conv, Func<TCustom, string> unit)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlFloat>)cb).AddConverter(
                new TomlConverter<TCustom, TomlFloat>((root, customValue) =>
                {
                    return new TomlFloat(root, conv(customValue))
                    {
                        Unit = unit(customValue),
                    };
                }));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlString> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlString> cb, Func<TCustom, string> conv, Func<TCustom, string> unit)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlString>)cb).AddConverter(
                new TomlConverter<TCustom, TomlString>((root, customValue) =>
                {
                    return new TomlString(root, conv(customValue))
                    {
                        Unit = unit(customValue),
                    };
                }));
            return cb;
        }
    }
}
