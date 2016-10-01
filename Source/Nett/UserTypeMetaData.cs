namespace Nett
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Util;

    internal static class UserTypeMetaData
    {
        private static readonly ConcurrentDictionary<Type, MetaDataInfo> MetaData = new ConcurrentDictionary<Type, MetaDataInfo>();

        public static bool IsPropertyIgnored(Type ownerType, PropertyInfo pi)
        {
            if (ownerType == null) { throw new ArgumentNullException(nameof(ownerType)); }
            if (pi == null) { throw new ArgumentNullException(nameof(pi)); }

            EnsureMetaDataInitialized(ownerType);

            return MetaData[ownerType].IgnoredProperties.Contains(pi.Name);
        }

        private static void EnsureMetaDataInitialized(Type t)
            => Extensions.DictionaryExtensions.AddIfNeeded(MetaData, t, () => ProcessType(t));

        private static MetaDataInfo ProcessType(Type t)
        {
            var ignored = ProcessIgnoredProperties(t);

            return new MetaDataInfo(ignored);
        }

        private static IEnumerable<string> ProcessIgnoredProperties(Type t)
        {
            var attributes = ReflectionUtil.GetPropertiesWithAttribute<TomlIgnoreAttribute>(t);
            return attributes.Select(pi => pi.Name);
        }

        private sealed class MetaDataInfo
        {
            public MetaDataInfo(IEnumerable<string> ignoredProperties)
            {
                this.IgnoredProperties = new HashSet<string>(ignoredProperties);
            }

            public HashSet<string> IgnoredProperties { get; }
        }
    }
}
