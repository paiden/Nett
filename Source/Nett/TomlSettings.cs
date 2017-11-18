namespace Nett
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using static System.Diagnostics.Debug;

    internal enum TomlCommentLocation
    {
        Prepend,
        Append,
    }

    public sealed partial class TomlSettings
    {
        internal static readonly TomlSettings DefaultInstance = Create();

        private const BindingFlags PropBindingFlags = BindingFlags.Public | BindingFlags.Instance;

        private readonly Dictionary<Type, Func<object>> activators = new Dictionary<Type, Func<object>>();
        private readonly ConverterCollection converters = new ConverterCollection();
        private readonly HashSet<Type> inlineTableTypes = new HashSet<Type>();
        private readonly Dictionary<string, Type> tableKeyToTypeMappings = new Dictionary<string, Type>();
        private readonly Dictionary<Type, HashSet<string>> ignoredProperties = new Dictionary<Type, HashSet<string>>();

        private TomlCommentLocation defaultCommentLocation = TomlCommentLocation.Prepend;

        internal TomlString.TypeOfString DefaultStringType { get; private set; } = TomlString.TypeOfString.Auto;

        private TomlSettings()
        {
        }

        public static TomlSettings Create() => Create(_ => { });

        public static TomlSettings Create(Action<ITomlSettingsBuilder> cfg)
        {
            var config = new TomlSettings();
            var builder = new TomlSettingsBuilder(config);
            cfg(builder);
            builder.SetupConverters();
            return config;
        }

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
                    throw new InvalidOperationException(string.Format(
                        "Failed to create type '{1}'. Only types with a " +
                        "parameterless constructor or an specialized creator can be created. Make sure the type has " +
                        "a parameterless constructor or a configuration with an corresponding creator is provided.",
                        exc.Message,
                        t.FullName));
                }
            }
        }

        internal TomlCommentLocation GetCommentLocation(TomlComment c)
        {
            switch (c.Location)
            {
                case CommentLocation.Append: return TomlCommentLocation.Append;
                case CommentLocation.Prepend: return TomlCommentLocation.Prepend;
                default: return this.defaultCommentLocation;
            }
        }

        internal TomlTable.TableTypes GetTableType(Type valType)
        {
            if (this.inlineTableTypes.Contains(valType)
                || valType.GetCustomAttributes(false).Any((a) => a.GetType() == typeof(TreatAsInlineTableAttribute)))
            {
                return TomlTable.TableTypes.Inline;
            }

            return TomlTable.TableTypes.Default;
        }

        internal IEnumerable<PropertyInfo> GetSerializationProperties(Type t)
        {
            return t.GetProperties(PropBindingFlags)
                .Where(pi => !this.IsPropertyIgnored(t, pi));
        }

        internal PropertyInfo TryGetMappingProperty(Type t, string key)
        {
            var pi = t.GetProperty(key, PropBindingFlags);
            return pi != null && !this.IsPropertyIgnored(t, pi) ? pi : null;
        }

        internal ITomlConverter TryGetConverter(Type from, Type to) =>
            this.converters.TryGetConverter(from, to);

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

        internal ITomlConverter TryGetToTomlConverter(Type fromType) =>
            this.converters.TryGetLatestToTomlConverter(fromType);

        private bool IsPropertyIgnored(Type ownerType, PropertyInfo pi)
        {
            Assert(ownerType != null);
            Assert(pi != null);

            HashSet<string> ignored;

            bool contained = UserTypeMetaData.IsPropertyIgnored(ownerType, pi);
            if (contained) { return true; }

            if (this.ignoredProperties.TryGetValue(ownerType, out ignored))
            {
                return ignored.Contains(pi.Name);
            }

            return false;
        }

        private void AddConverter(ITomlConverter converter) =>
                                                                            this.converters.Add(converter);

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
