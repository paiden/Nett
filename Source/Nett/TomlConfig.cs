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
        private readonly Dictionary<string, Type> tableKeyToTypeMappings = new Dictionary<string, Type>();

        private TomlCommentLocation DefaultCommentLocation = TomlCommentLocation.Prepend;
        private TomlConfig()
        {
        }

        private void AddConverter(ITomlConverter converter) =>
            this.converters.Add(converter);

        public static TomlConfig Create() => Create(_ => { });

        public static TomlConfig Create(Action<ITomlConfigBuilder> cfg)
        {
            var config = new TomlConfig();
            var builder = new TomlConfigBuilder(config);
            cfg(builder);
            builder.ApplyConversionSettings(); //Apply last, so that default converters get registered last and  only once
            return config;
        }

        internal Type TryGetMappedType(string key, PropertyInfo target)
        {
            Type mapped;
            bool noTypeInfoAvailable = target == null;
            bool targetCanHoldMappedTable = noTypeInfoAvailable || target.PropertyType == Types.ObjectType;
            if (targetCanHoldMappedTable && this.tableKeyToTypeMappings.TryGetValue(key, out mapped))
            {
                return mapped;
            }

            return null;
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

        internal TomlTable.TableTypes GetTableType(PropertyInfo pi)
        {
            if (pi == null) { return TomlTable.TableTypes.Default; }

            return this.inlineTableTypes.Contains(pi.PropertyType) || pi.GetCustomAttributes(false).Any((a) => a.GetType() == typeof(TomlInlineTableAttribute))
                ? TomlTable.TableTypes.Inline
                : TomlTable.TableTypes.Default;
        }

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
            private readonly List<ITomlConverter> converters = new List<ITomlConverter>();

            public void Add(ITomlConverter converter) => this.converters.Insert(0, converter);
            public void AddRange(IEnumerable<ITomlConverter> converters) => this.converters.InsertRange(0, converters);

            public ITomlConverter TryGetConverter(Type from, Type to) => this.converters.FirstOrDefault(c => c.CanConvertFrom(from) && c.CanConvertTo(to));

            public ITomlConverter TryGetLatestToTomlConverter(Type from) =>
                this.converters.FirstOrDefault(c => c.CanConvertFrom(from) && c.CanConvertToToml());
        }
    }
}
