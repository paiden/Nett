using System;
using System.Reflection;
using Nett.Util;
using static System.Diagnostics.Debug;

namespace Nett.Mapping
{
    internal struct SerializationMember : IEquatable<SerializationMember>
    {
        private readonly PropertyInfo pi;
        private readonly FieldInfo fi;

        public SerializationMember(PropertyInfo pi)
        {
            this.fi = null;
            this.pi = pi;
        }

        public SerializationMember(FieldInfo fi)
        {
            this.pi = null;
            this.fi = fi;
        }

        public MemberInfo MemberInfo
            => this.pi ?? (MemberInfo)this.fi;

        public Type MemberType
            => this.pi?.PropertyType ?? this.fi.FieldType;

        public static bool operator ==(SerializationMember x, SerializationMember y)
            => x.Equals(y);

        public static bool operator !=(SerializationMember x, SerializationMember y)
            => !x.Equals(y);

        public object GetValue(object instance)
        {
            Assert(this.pi != null || this.fi != null);

            if (this.pi != null)
            {
                return this.pi.GetValue(instance, null);
            }
            else
            {
                return this.fi.GetValue(instance);
            }
        }

        public TomlKey GetKey()
        {
            Assert(this.pi != null || this.fi != null);

            string keyString = this.pi?.Name ?? this.fi.Name;

            return new TomlKey(keyString, TomlKey.KeyType.Bare);
        }

        public bool Is(MemberInfo pi)
            => this.pi != null && this.pi == pi;

        public void SetValue(object instance, object value)
        {
            if (this.pi != null) { this.pi.SetValue(instance, value, null); }
            else { this.fi.SetValue(instance, value); }
        }

        public override bool Equals(object obj)
            => obj is SerializationMember m && this.Equals(m);

        public override int GetHashCode()
            => HashUtil.GetHashCode(this.pi, this.fi);

        public bool Equals(SerializationMember other)
            => this.pi == other.pi && this.fi == other.fi;
    }
}
