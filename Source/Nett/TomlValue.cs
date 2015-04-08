using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public abstract class TomlValue : TomlObject
    {
        protected static readonly Type Int32Type = typeof(int);
        protected static readonly Type Int16Type = typeof(short);
        protected static readonly Type Int64Type = typeof(long);
        protected static readonly Type CharType = typeof(char);
        protected static readonly Type UInt16Type = typeof(ushort);
        protected static readonly Type UInt32Type = typeof(uint);
        protected static readonly Type StringType = typeof(string);

        private static readonly Type[] IntTypes = new Type[]
            {
                CharType,
                Int16Type, Int32Type, Int64Type,
                UInt16Type, UInt32Type,
            };

        public static TomlValue From(object val, Type t)
        {
            if(StringType == t)
            {
                return new TomlValue<string>((string)val);
            }
            if(IsIntegerType(t))
            {
                return new TomlValue<long>((long)Convert.ChangeType(val, Int64Type));
            }

            throw new NotSupportedException(string.Format("Cannot create TOML value from '{0}'.", t.FullName));
        }

        private static bool IsIntegerType(Type t)
        {
            for(int i = 0; i < IntTypes.Length; i++)
            {
                if(IntTypes[i] == t)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class TomlValue<T> : TomlValue
    {
        private static readonly Type ValueType = typeof(T);
        private readonly T value;
        public T Value => this.value;

        public TomlValue(T value)
        {
            this.value = value;
        }

        public override TRes Get<TRes>()
        {
            return this.Get<TRes>(TomlConfig.DefaultInstance);
        }

        public override TRes Get<TRes>(TomlConfig config)
        {
            return Converter.Convert<TRes>(this.Value);
        }

        public override object Get(Type t)
        {
            return this.Get(t, TomlConfig.DefaultInstance);
        }

        public override object Get(Type t, TomlConfig config)
        {
            return Converter.Convert(t, this.Value);
        }

        public override void WriteTo(StreamWriter writer)
        {
            this.WriteTo(writer, TomlConfig.DefaultInstance);
        }

        public override void WriteTo(StreamWriter writer, TomlConfig config)
        {
            if (ValueType == StringType)
            {
                writer.Write(@"""{0}""", (string)(object)this.Value ?? "");
            }
            else
            {
                writer.Write(this.Value);
            }
        }
    }
}
