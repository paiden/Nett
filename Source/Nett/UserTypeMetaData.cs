using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nett.Mapping;
using Nett.Util;

namespace Nett
{
    internal static class StaticTypeMetaData
    {
        private static readonly ConcurrentDictionary<Type, MetaDataInfo> MetaData = new ConcurrentDictionary<Type, MetaDataInfo>();

        public static IEnumerable<SerializationInfo> GetSerializationMembers(Type type, IKeyGenerator keyGen)
        {
            EnsureMetaDataInitialized(type);

            var data = MetaData[type];

            return data.ImplicitMembers
                .Select(sm => new SerializationInfo(sm, new TomlKey(keyGen.GetKey(sm.MemberInfo))))
                .Concat(data.ExplicitMembers);
        }

        public static IEnumerable<TomlComment> GetComments(Type type, SerializationMember mi)
        {
            EnsureMetaDataInitialized(type);
            if (MetaData[type].Comments.TryGetValue(mi, out var c))
            {
                return c;
            }

            return Enumerable.Empty<TomlComment>();
        }

        public static bool IsMemberIgnored(Type t, MemberInfo mi)
        {
            EnsureMetaDataInitialized(t);

            return MetaData[t].IgnoredMembes.Any(m => m.Is(mi));
        }

        private static void EnsureMetaDataInitialized(Type t)
            => Extensions.DictionaryExtensions.AddIfNeeded(MetaData, t, () => ProcessType(t));

        private static MetaDataInfo ProcessType(Type t)
        {
            var implicitMembers = new HashSet<SerializationMember>(ResolveImplicitMembers(t));
            var explicitMembers = new HashSet<SerializationInfo>(ResolveExplicitMembers(t));
            var ignoredMembers = new HashSet<SerializationMember>(ResolveIgnoredMembers(t));
            var comments = ResolveComments(implicitMembers.Concat(explicitMembers.Select(em => em.Member)));

            return new MetaDataInfo(implicitMembers, explicitMembers, ignoredMembers, comments);
        }

        private static Dictionary<SerializationMember, List<TomlComment>> ResolveComments(IEnumerable<SerializationMember> members)
        {
            return members.ToDictionary(m => m, GetComments);

            List<TomlComment> GetComments(SerializationMember sm)
                => ReflectionUtil.GetCustomAttributes<TomlCommentAttribute>(sm.MemberInfo, inherit: true)
                .Select(ca => new TomlComment(ca.Comment, ca.Location))
                .ToList();
        }

        private static IEnumerable<SerializationMember> ResolveImplicitMembers(Type t)
        {
            return t.GetProperties(TomlSettings.PropBindingFlags)
                .Where(IncludeMember)
                .Select(pi => new SerializationMember(pi));

            bool IncludeMember(PropertyInfo pi)
                => pi.GetIndexParameters().Length <= 0
                && ReflectionUtil.GetCustomAttribute<TomlIgnoreAttribute>(pi, inherit: true) == null
                && ReflectionUtil.GetCustomAttribute<TomlMember>(pi, inherit: true) == null;
        }

        private static IEnumerable<SerializationMember> ResolveIgnoredMembers(Type t)
        {
            return t.GetProperties(TomlSettings.PropBindingFlags)
                .Where(pi => ReflectionUtil.GetCustomAttribute<TomlIgnoreAttribute>(pi, inherit: true) != null)
                .Select(pi => new SerializationMember(pi));
        }

        private static IEnumerable<SerializationInfo> ResolveExplicitMembers(Type t)
        {
            var members = t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var m in members)
            {
                var tm = ReflectionUtil.GetCustomAttribute<TomlMember>(m);
                if (tm != null)
                {
                    var key = string.IsNullOrWhiteSpace(tm.Key)

                        ? new TomlKey(m.Name, TomlKey.KeyType.Bare)
                        : new TomlKey(tm.Key);

                    yield return SerializationInfo.CreateFromMemberInfo(m, key);
                }
            }
        }

        private sealed class MetaDataInfo
        {

            public MetaDataInfo(
                HashSet<SerializationMember> implicitMembers,
                HashSet<SerializationInfo> explicitMembers,
                HashSet<SerializationMember> ignoredMembers,
                Dictionary<SerializationMember, List<TomlComment>> comments)
            {
                this.ImplicitMembers = implicitMembers;
                this.ExplicitMembers = explicitMembers;
                this.IgnoredMembes = ignoredMembers;
                this.Comments = comments;
            }

            public HashSet<SerializationMember> ImplicitMembers { get; }

            public HashSet<SerializationInfo> ExplicitMembers { get; }

            public HashSet<SerializationMember> IgnoredMembes { get; }

            public Dictionary<SerializationMember, List<TomlComment>> Comments { get; }
        }
    }
}
