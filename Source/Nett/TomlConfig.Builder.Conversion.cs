namespace Nett
{
    using System;
    using System.Collections.Generic;

    public interface IConfigureConversionBuilder<TCustom, TToml>
        where TToml : TomlObject
    {
        IConfigureConversionBuilder<TCustom, TToml> FromToml(Func<IMetaDataStore, TToml, TCustom> convert);

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
                new TomlConverter<TCustom, TomlBool>((metaData, customValue) => new TomlBool(metaData, conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlInt> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlInt> cb, Func<TCustom, long> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlInt>)cb).AddConverter(
                new TomlConverter<TCustom, TomlInt>((metaData, customValue) => new TomlInt(metaData, conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlFloat> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlFloat> cb, Func<TCustom, double> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlFloat>)cb).AddConverter(
                new TomlConverter<TCustom, TomlFloat>((metaData, customValue) => new TomlFloat(metaData, conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlString> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlString> cb, Func<TCustom, string> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlString>)cb).AddConverter(
                new TomlConverter<TCustom, TomlString>((metaData, customValue) => new TomlString(metaData, conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlDateTime> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlDateTime> cb, Func<TCustom, DateTimeOffset> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlDateTime>)cb).AddConverter(
                new TomlConverter<TCustom, TomlDateTime>((metaData, customValue) => new TomlDateTime(metaData, conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlTimeSpan> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlTimeSpan> cb, Func<TCustom, TimeSpan> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlTimeSpan>)cb).AddConverter(
                new TomlConverter<TCustom, TomlTimeSpan>((metaData, customValue) => new TomlTimeSpan(metaData, conv(customValue))));
            return cb;
        }

        public static IConfigureConversionBuilder<TCustom, TomlTable> ToToml<TCustom>(
            this IConfigureConversionBuilder<TCustom, TomlTable> cb, Action<TCustom, TomlTable> conv)
        {
            ((TomlConfig.ConversionConfigurationBuilder<TCustom, TomlTable>)cb).AddConverter(
                new TomlConverter<TCustom, TomlTable>((metaData, customValue) =>
                {
                    var t = new TomlTable(metaData);
                    conv(customValue, t);
                    return t;
                }));

            return cb;
        }
    }

    public sealed partial class TomlConfig
    {
        private static readonly List<ITomlConverter> CastConverters = new List<ITomlConverter>()
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

        private static readonly List<ITomlConverter> StrictConverters = new List<ITomlConverter>()
            .AddBidirectionalConverter<TomlInt, long>((m, c) => new TomlInt(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlFloat, double>((m, c) => new TomlFloat(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlString, string>((m, c) => new TomlString(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlDateTime, DateTimeOffset>((m, c) => new TomlDateTime(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlTimeSpan, TimeSpan>((m, c) => new TomlTimeSpan(m, c), (m, t) => t.Value)
            .AddBidirectionalConverter<TomlBool, bool>((m, c) => new TomlBool(m, c), (m, t) => t.Value);

        private static readonly List<ITomlConverter> ConvertConverters = new List<ITomlConverter>()
        {
            // TomlStrings <-> enums
            new TomlToEnumConverter(),
            new EnumToTomlConverter(),

            // Dict <-> TomlTable
            new TomlTableToDictionaryConverter(),
            new TomlTableToTypedDictionaryConverter(),
        }
        .AddBidirectionalConverter<TomlString, Guid>((m, c) => new TomlString(m, c.ToString("D")), (m, t) => Guid.Parse(t.Value));

        [Flags]
        public enum ConversionSets
        {
            Strict = 1 << 0,
            Cast = 1 << 1,
            Convert = 1 << 2,

            All = Strict | Cast | Convert,
        }
    }

    internal static class RegisterConverterExtensions
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
