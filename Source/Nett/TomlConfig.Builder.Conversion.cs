using System;

namespace Nett
{
    public interface IConfigureConversionBuilder<TCustom, TToml> where TToml : TomlObject
    {
        IConfigureConversionBuilder<TCustom, TToml> ToToml(Func<TCustom, TToml> convert);
        IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<TToml, TCustom> convert);
    }


    /// <summary>
    /// This class provides generic specializations for IConfigureConversionBuilder.
    /// </summary>
    /// <remarks>
    /// These specializations are used, so that the user supplying the conversion
    /// doesn't need to invoke the TOML object constructor directly which he cannot
    /// as it is internal.
    /// </remarks>
    public static class ConversionBuilderExtensions
    {
        public static IConfigureConversionBuilder<TCustom, TomlBool> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlBool> cb, Func<TCustom, bool> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlBool>)cb).AddConverter(
                new TomlConverter<TCustom, TomlBool>(customValue => new TomlBool(conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlInt> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlInt> cb, Func<TCustom, long> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlInt>)cb).AddConverter(
                new TomlConverter<TCustom, TomlInt>(customValue => new TomlInt(conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlFloat> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlFloat> cb, Func<TCustom, double> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlFloat>)cb).AddConverter(
                new TomlConverter<TCustom, TomlFloat>(customValue => new TomlFloat(conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlString> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlString> cb, Func<TCustom, string> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlString>)cb).AddConverter(
                new TomlConverter<TCustom, TomlString>(customValue => new TomlString(conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlDateTime> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlDateTime> cb, Func<TCustom, DateTimeOffset> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlDateTime>)cb).AddConverter(
                new TomlConverter<TCustom, TomlDateTime>(customValue => new TomlDateTime(conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlTimeSpan> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlTimeSpan> cb, Func<TCustom, TimeSpan> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlTimeSpan>)cb).AddConverter(
                new TomlConverter<TCustom, TomlTimeSpan>(customValue => new TomlTimeSpan(conv(customValue))));
            return cb;
        }
    }
}
