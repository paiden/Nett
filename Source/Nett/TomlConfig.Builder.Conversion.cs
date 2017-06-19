namespace Nett
{
    using System;
    using System.Collections.Generic;

    public interface IConversionSettingsBuilder<TCustom, TToml>
        where TToml : TomlObject
    {
        IConversionSettingsBuilder<TCustom, TToml> FromToml(Func<ITomlRoot, TToml, TCustom> convert);

        IConversionSettingsBuilder<TCustom, TToml> FromToml(Func<TToml, TCustom> convert);
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
        public static IConversionSettingsBuilder<TCustom, TomlBool> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlBool> cb, Func<TCustom, bool> conv)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlBool>)cb).AddConverter(
                new TomlConverter<TCustom, TomlBool>((root, customValue) => new TomlBool(root, conv(customValue))));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlInt> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlInt> cb, Func<TCustom, long> conv)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlInt>)cb).AddConverter(
                new TomlConverter<TCustom, TomlInt>((root, customValue) => new TomlInt(root, conv(customValue))));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlFloat> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlFloat> cb, Func<TCustom, double> conv)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlFloat>)cb).AddConverter(
                new TomlConverter<TCustom, TomlFloat>((root, customValue) => new TomlFloat(root, conv(customValue))));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlString> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlString> cb, Func<TCustom, string> conv)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlString>)cb).AddConverter(
                new TomlConverter<TCustom, TomlString>((root, customValue) => new TomlString(root, conv(customValue))));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlDateTime> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlDateTime> cb, Func<TCustom, DateTimeOffset> conv)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlDateTime>)cb).AddConverter(
                new TomlConverter<TCustom, TomlDateTime>((root, customValue) => new TomlDateTime(root, conv(customValue))));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlTimeSpan> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlTimeSpan> cb, Func<TCustom, TimeSpan> conv)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlTimeSpan>)cb).AddConverter(
                new TomlConverter<TCustom, TomlTimeSpan>((root, customValue) => new TomlTimeSpan(root, conv(customValue))));
            return cb;
        }

        public static IConversionSettingsBuilder<TCustom, TomlTable> ToToml<TCustom>(
            this IConversionSettingsBuilder<TCustom, TomlTable> cb, Action<TCustom, TomlTable> conv)
        {
            ((TomlSettings.ConversionSettingsBuilder<TCustom, TomlTable>)cb).AddConverter(
                new TomlConverter<TCustom, TomlTable>((root, customValue) =>
                {
                    var t = new TomlTable(root);
                    conv(customValue, t);
                    return t;
                }));

            return cb;
        }
    }

    public sealed partial class TomlSettings
    {
        private static readonly List<ITomlConverter> NumercialType = new List<ITomlConverter>()
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

            // TOML -> CLR
            // TomlInt -> *
            new TomlConverter<TomlInt, float>((m, i) => i.Value),
            new TomlConverter<TomlInt, double>((m, i) => i.Value),
        }
        .AddBidirectionalConverter<TomlInt, TomlFloat>((m, f) => new TomlInt(m, (long)f.Value), (m, i) => new TomlFloat(m, i.Value));

        private static readonly List<ITomlConverter> NumericalSize = new List<ITomlConverter>()
        {
            // TOML -> CLR
            // TomlFloat -> *
            new TomlConverter<TomlFloat, float>((m, f) => (float)f.Value),

            // TomlInt -> *
            new TomlConverter<TomlInt, ulong>((m, i) => (ulong)i.Value),
            new TomlConverter<TomlInt, int>((m, i) => (int)i.Value),
            new TomlConverter<TomlInt, uint>((m, i) => (uint)i.Value),
            new TomlConverter<TomlInt, short>((m, i) => (short)i.Value),
            new TomlConverter<TomlInt, ushort>((m, i) => (ushort)i.Value),
            new TomlConverter<TomlInt, char>((m, i) => (char)i.Value),
            new TomlConverter<TomlInt, byte>((m, i) => (byte)i.Value),

            // CLR -> TOML
            // * -> TomlInt
            new TomlConverter<ulong, TomlInt>((m, v) => new TomlInt(m, (long)v)),
            new TomlConverter<uint, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<int, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<short, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<ushort, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<char, TomlInt>((m, v) => new TomlInt(m, v)),
            new TomlConverter<byte, TomlInt>((m, v) => new TomlInt(m, v)),

            // * -> TomlFloat
            new TomlConverter<float, TomlFloat>((m, v) => new TomlFloat(m, v)),
        }
        .AddBidirectionalConverter<TomlDateTime, DateTime>((m, c) => new TomlDateTime(m, c), (m, t) => t.Value.UtcDateTime);

        // Without these converters the library will not work correctly
        private static readonly List<ITomlConverter> EquivalentTypeConverters = new List<ITomlConverter>()
        {
            new TomlTableToDictionaryConverter(),
            new TomlTableToTypedDictionaryConverter(),
        }
        .AddBidirectionalConverter<TomlInt, long>((m, c) => new TomlInt(m, c), (m, t) => t.Value)
        .AddBidirectionalConverter<TomlFloat, double>((m, c) => new TomlFloat(m, c), (m, t) => t.Value)
        .AddBidirectionalConverter<TomlString, string>((m, c) => new TomlString(m, c), (m, t) => t.Value)
        .AddBidirectionalConverter<TomlDateTime, DateTimeOffset>((m, c) => new TomlDateTime(m, c), (m, t) => t.Value)
        .AddBidirectionalConverter<TomlTimeSpan, TimeSpan>((m, c) => new TomlTimeSpan(m, c), (m, t) => t.Value)
        .AddBidirectionalConverter<TomlBool, bool>((m, c) => new TomlBool(m, c), (m, t) => t.Value);

        private static readonly List<ITomlConverter> SerializeConverters = new List<ITomlConverter>()
        {
            new TomlToEnumConverter(),
            new EnumToTomlConverter(),
        }
        .AddBidirectionalConverter<TomlString, Guid>((m, c) => new TomlString(m, c.ToString("D")), (m, t) => Guid.Parse(t.Value));

        [Flags]
        public enum ConversionSets
        {
            None = 0,

            NumericalSize = 1 << 0,
            Serialize = 1 << 1,
            NumericalType = 1 << 2,

            All = NumericalSize | NumericalType | Serialize,
            Default = NumericalSize | Serialize,
        }
    }

    internal static class RegisterConverterExtensions
    {
        public static List<ITomlConverter> AddBidirectionalConverter<TToml, TClr>(
            this List<ITomlConverter> converterlist,
            Func<ITomlRoot, TClr, TToml> toToml,
            Func<ITomlRoot, TToml, TClr> toClr)
            where TToml : TomlObject
        {
            converterlist.Add(new TomlConverter<TToml, TClr>(toClr));
            converterlist.Add(new TomlConverter<TClr, TToml>(toToml));
            return converterlist;
        }
    }
}
