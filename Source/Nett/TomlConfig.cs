using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nett
{
    enum TomlCommentLocation
    {
        Prepend,
        Append,
    }

    public sealed partial class TomlConfig
    {
        internal static readonly TomlConfig DefaultInstance = Create();

        private readonly ConverterCollection converters = new ConverterCollection();
        private readonly Dictionary<Type, Func<object>> activators = new Dictionary<Type, Func<object>>();
        private readonly HashSet<Type> inlineTableTypes = new HashSet<Type>();

        private TomlCommentLocation DefaultCommentLocation = TomlCommentLocation.Prepend;
        private TomlConfig()
        {
            // TomlInt
            this.AddConverter(new TomlConverter<TomlInt, char>(t => (char)t.Value));
            this.AddConverter(new TomlConverter<TomlInt, byte>(t => (byte)t.Value));
            this.AddConverter(new TomlConverter<TomlInt, int>(t => (int)t.Value));
            this.AddConverter(new TomlConverter<TomlInt, short>(t => (short)t.Value));
            this.AddConverter(new TomlConverter<TomlInt, long>(t => (long)t.Value));

            // TomlFloat
            this.AddConverter(new TomlConverter<TomlFloat, double>(t => t.Value));
            this.AddConverter(new TomlConverter<TomlFloat, float>(t => (float)t.Value));

            // TomlString
            this.AddConverter(new TomlConverter<TomlString, string>(t => t.Value));

            // TomlDateTime
            this.AddConverter(new TomlConverter<TomlDateTime, DateTime>(t => t.Value.UtcDateTime));
            this.AddConverter(new TomlConverter<TomlDateTime, DateTimeOffset>(t => t.Value));

            // TomlTimeSpan
            this.AddConverter(new TomlConverter<TomlTimeSpan, TimeSpan>(t => t.Value));

            // TomlBool
            this.AddConverter(new TomlConverter<TomlBool, bool>(t => t.Value));
        }

        private void AddConverter(ITomlConverter converter) =>
            this.converters.Add(converter);

        public static TomlConfig Create() => new TomlConfig();

        public static TomlConfig Create(Action<ITomlConfigBuilder> cfg)
        {
            var config = new TomlConfig();
            cfg(new TomlConfigBuilder(config));
            return config;
        }

        internal ITomlConverter TryGetConverter(Type from, Type to) =>
            this.converters.TryGetConverter(from, to);

        internal ITomlConverter TryGetToTomlConverter(Type fromType) =>
            this.converters.TryGetLatestToTomlConverter(fromType);

        internal object GetActivatedInstance(Type t)
        {
            Func<object> a;
            if (this.activators.TryGetValue(t, out a))
            {
                return a();
            }
            else
            {
                try
                {
                    return Activator.CreateInstance(t);
                }
                catch (MissingMethodException exc)
                {
                    throw new InvalidOperationException(string.Format("Failed to create type '{1}'. Only types with a " +
                        "parameterless constructor or an specialized creator can be created. Make sure the type has " +
                        "a parameterless constructor or a configuration with an corresponding creator is provided.", exc.Message, t.FullName));
                }
            }
        }

        internal TomlTable.TableTypes GetTableType(PropertyInfo pi) =>
            this.inlineTableTypes.Contains(pi.PropertyType) ||
            pi.GetCustomAttributes(false).Any((a) => a.GetType() == typeof(TomlInlineTableAttribute)) ? TomlTable.TableTypes.Inline : TomlTable.TableTypes.Default;

        internal TomlCommentLocation GetCommentLocation(TomlComment c)
        {
            switch (c.Location)
            {
                case CommentLocation.Append: return TomlCommentLocation.Append;
                case CommentLocation.Prepend: return TomlCommentLocation.Prepend;
                default: return DefaultCommentLocation;
            }
        }

        private class ConverterCollection
        {
            private readonly Dictionary<Type, Dictionary<Type, ITomlConverter>> convertFromTypeToTypeMapping = new Dictionary<Type, Dictionary<Type, ITomlConverter>>();
            private readonly Dictionary<Type, List<ITomlConverter>> convertFromToTomlConverters = new Dictionary<Type, List<ITomlConverter>>();

            public void Add(ITomlConverter converter)
            {
                Dictionary<Type, ITomlConverter> val;
                if (!convertFromTypeToTypeMapping.TryGetValue(converter.FromType, out val))
                {
                    val = convertFromTypeToTypeMapping[converter.FromType] = new Dictionary<Type, ITomlConverter>();
                }

                val[converter.ToType] = converter;

                if (typeof(TomlObject).IsAssignableFrom(converter.ToType))
                {
                    List<ITomlConverter> tmlConverters;
                    if (!this.convertFromToTomlConverters.TryGetValue(converter.FromType, out tmlConverters))
                    {
                        tmlConverters = this.convertFromToTomlConverters[converter.FromType] = new List<ITomlConverter>();
                    }

                    tmlConverters.Add(converter);
                }
            }

            public ITomlConverter TryGetConverter(Type from, Type to)
            {
                Dictionary<Type, ITomlConverter> toConverters;
                if (this.convertFromTypeToTypeMapping.TryGetValue(from, out toConverters))
                {
                    ITomlConverter conv;
                    if (toConverters.TryGetValue(to, out conv))
                    {
                        return conv;
                    }
                }

                return null;
            }

            public ITomlConverter TryGetLatestToTomlConverter(Type from)
            {
                List<ITomlConverter> converters;
                if (this.convertFromToTomlConverters.TryGetValue(from, out converters))
                {
                    return converters.Last();
                }

                return null;
            }
        }
    }
}
