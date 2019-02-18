using System;
using System.Reflection;
using Nett.Util;

namespace Nett.Mapping
{
    internal struct SerializationInfo : IEquatable<SerializationInfo>
    {
        public SerializationInfo(PropertyInfo pi, TomlKey key)
        {
            this.Member = new SerializationMember(pi);
            this.Key = key;
        }

        public SerializationInfo(FieldInfo fi, TomlKey key)
        {
            this.Member = new SerializationMember(fi);
            this.Key = key;
        }

        public SerializationInfo(SerializationMember si, TomlKey key)
        {
            this.Member = si;
            this.Key = key;
        }

        public TomlKey Key { get; }

        public SerializationMember Member { get; }

        public static bool operator ==(SerializationInfo x, SerializationInfo y)
            => x.Equals(y);

        public static bool operator !=(SerializationInfo x, SerializationInfo y)
            => !x.Equals(y);

        public static SerializationInfo CreateFromMemberInfo(MemberInfo mi, TomlKey key)
        {
            if (mi is PropertyInfo pi) { return new SerializationInfo(pi, key); }
            else if (mi is FieldInfo fi) { return new SerializationInfo(fi, key); }
            else { throw new ArgumentException($"Cannot create serialization info from unsupported member info type '{mi.GetType()}'."); }
        }

        public object GetValue(object instance)
            => this.Member.GetValue(instance);

        public bool Is(MemberInfo pi)
            => this.Member.Is(pi);

        public override bool Equals(object obj)
            => obj is SerializationInfo si && this.Equals(si);

        public bool Equals(SerializationInfo other)
            => this.Key == other.Key && this.Member == other.Member;

        public override int GetHashCode()
            => HashUtil.GetHashCode(this.Key, this.Member);
    }
}
