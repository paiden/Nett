namespace Nett
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Nett.Collections;
    using Nett.Mapping;
    using static System.Diagnostics.Debug;

    internal enum TomlCommentLocation
    {
        Prepend,
        Append,
    }

    internal enum ExperimentalFeature
    {
        ValueWithUnit,
    }

    public sealed partial class TomlSettings
    {
        internal const BindingFlags PropBindingFlags = BindingFlags.Public | BindingFlags.Instance;

        internal static readonly TomlSettings DefaultInstance = Create();

        private readonly Dictionary<Type, Func<CreateInstanceContext, object>> activators = new Dictionary<Type, Func<CreateInstanceContext, object>>();
        private readonly ConverterCollection converters = new ConverterCollection();
        private readonly HashSet<Type> inlineTableTypes = new HashSet<Type>();
        private readonly Dictionary<string, Type> tableKeyToTypeMappings = new Dictionary<string, Type>();
        private readonly Dictionary<Type, HashSet<SerializationMember>> ignoredMembers = new Dictionary<Type, HashSet<SerializationMember>>();
        private readonly Map<SerializationInfo, string> explicitMembers = new Map<SerializationInfo, string>();
        private readonly Dictionary<ExperimentalFeature, bool> featureFlags = new Dictionary<ExperimentalFeature, bool>();

        private IKeyGenerator keyGenerator = KeyGenerators.Instance.PropertyName;
        private ITargetPropertySelector mappingPropertySelector = TargetPropertySelectors.Instance.Exact;
        private Action<string[], object, TomlObject> whenTargetPropertyNotFoundCallback = (_, __, ___) => { };

        private TomlCommentLocation defaultCommentLocation = TomlCommentLocation.Prepend;

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

        internal bool TryGetUserActivatedInstance(Type t, CreateInstanceContext context, out object activated)
        {
            activated = null;

            if (this.activators.TryGetValue(t, out var a))
            {
                activated = a(context);
                return true;
            }

            return false;
        }

        internal object GetActivatedInstance(Type t, CreateInstanceContext context)
        {
            if (this.activators.TryGetValue(t, out var a))
            {
                return a(context);
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

        internal IEnumerable<SerializationInfo> GetSerializationMembers(Type t)
        {
            return StaticTypeMetaData.GetSerializationMembers(t, this.keyGenerator)
                .Where(si => IncludeMember(si.Member.MemberInfo))
                .Concat(this.explicitMembers.Forward.Keys);

            bool IncludeMember(MemberInfo mi)
            {
                return !this.IsMemberIgnored(t, mi)
                    && !this.explicitMembers.Forward.Keys.Any(si => si.Is(mi));
            }
        }

        internal IEnumerable<TomlComment> GetComments(Type type, SerializationMember m)
            => StaticTypeMetaData.GetComments(type, m);

        internal SerializationMember? TryGetMappedMember(Type t, string key)
        {
            var configured = StaticTypeMetaData.GetSerializationMembers(t, this.keyGenerator)
                .Concat(this.explicitMembers.Forward.Keys);

            var cmem = configured.Where(IsFlup)
                .Cast<SerializationInfo?>()
                .FirstOrDefault();

            bool IsFlup(SerializationInfo si)
            {
                bool r = si.Key.Value == key;
                return r;
            }

            if (cmem.HasValue && !this.IsMemberIgnored(t, cmem.Value.Member.MemberInfo))
            {
                return cmem.Value.Member;
            }

            var pi = this.mappingPropertySelector.TryGetTargetProperty(key, t);
            return pi != null && !this.IsMemberIgnored(t, pi) ? new SerializationMember(pi) : (SerializationMember?)null;
        }

        internal void OnTargetPropertyNotFound(string[] keyChain, object target, TomlObject value)
            => this.whenTargetPropertyNotFoundCallback(keyChain, target, value);

        internal ITomlConverter TryGetConverter(TomlObject tomlObj, Type to)
            => this.TryGetConverter(tomlObj.GetType(), to);

        internal ITomlConverter TryGetConverter(Type from, Type to) =>
            this.converters.TryGetConverter(from, to);

        internal Type TryGetMappedType(string key, SerializationMember? target)
        {
            bool targetCanHoldMappedTable = !target.HasValue || target.Value.MemberType == Types.ObjectType;
            if (targetCanHoldMappedTable && this.tableKeyToTypeMappings.TryGetValue(key, out var mapped))
            {
                return mapped;
            }

            return null;
        }

        internal bool IsFeautureEnabled(ExperimentalFeature feature)
            => this.featureFlags.TryGetValue(feature, out bool flag) ? flag : false;

        internal ITomlConverter TryGetToTomlConverter(Type fromType) =>
            this.converters.TryGetLatestToTomlConverter(fromType);

        private bool IsMemberIgnored(Type ownerType, MemberInfo mi)
        {
            Assert(ownerType != null);
            Assert(mi != null);

            if (StaticTypeMetaData.IsMemberIgnored(ownerType, mi))
            {
                return true;
            }

            if (this.ignoredMembers.TryGetValue(ownerType, out var ignored))
            {
                return ignored.Any(m => m.Is(mi));
            }

            return false;
        }

        private sealed class ConverterCollection
        {
            private static readonly ToMatchingClrTypeConverter DirectConv = new ToMatchingClrTypeConverter();
            private readonly List<ITomlConverter> converters = new List<ITomlConverter>(64);

            public ConverterCollection()
            {
                this.converters.Add(DirectConv);
            }

            public void Add(ITomlConverter converter)
                => this.converters.Insert(1, converter);

            public void AddRange(IEnumerable<ITomlConverter> converters) => this.converters.InsertRange(1, converters);

            public ITomlConverter TryGetConverter(Type from, Type to) => this.converters.FirstOrDefault(c => c.CanConvertFrom(from) && c.CanConvertTo(to));

            public ITomlConverter TryGetLatestToTomlConverter(Type from) =>
                this.converters.FirstOrDefault(c => c.CanConvertFrom(from) && c.CanConvertToToml());

            private class ToMatchingClrTypeConverter : ITomlConverter<TomlObject, object>
            {
                public Type FromType
                    => typeof(TomlObject);

                public TomlObjectType? TomlTargetType => null;

                public bool CanConvertFrom(Type t)
                    => Types.TomlObjectType.IsAssignableFrom(t);

                public bool CanConvertTo(Type t)
                    => t == typeof(object);

                public bool CanConvertToToml()
                    => false;

                public object Convert(ITomlRoot root, TomlObject src, Type targetType)
                    => this.Convert(root, (object)src, targetType);

                public object Convert(ITomlRoot root, object value, Type targetType)
                {
                    switch (value)
                    {
                        case TomlValue val: return val.UntypedValue;
                        case TomlTable tbl: return tbl.ToDictionary();
                        case TomlTableArray tarr: return tarr.Items.Select(i => this.Convert(root, i, typeof(object)));
                        default: return value;
                    }
                }
            }
        }
    }
}
