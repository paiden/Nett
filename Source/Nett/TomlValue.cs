using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        protected static readonly Type TimespanType = typeof(TimeSpan);
        protected static readonly Type FloatType = typeof(float);
        protected static readonly Type DoubleType = typeof(double);

        private static readonly Type[] IntTypes = new Type[]
            {
                CharType,
                Int16Type, Int32Type, Int64Type,
                UInt16Type, UInt32Type,
            };

        public static TomlValue From(object val, Type targetType)
        {
            if(StringType == targetType)
            {
                return new TomlValue<string>((string)val);
            }
            else if(TimespanType == targetType)
            {
                return new TomlValue<TimeSpan>((TimeSpan)val);
            }
            else if(IsFloatType(targetType))
            {
                return new TomlValue<double>((double)Convert.ChangeType(val, DoubleType));
            }
            else if(IsIntegerType(targetType))
            {
                return new TomlValue<long>((long)Convert.ChangeType(val, Int64Type));
            }

            throw new NotSupportedException(string.Format("Cannot create TOML value from '{0}'.", targetType.FullName));
        }

        public static TomlValue<T> From<T>(T value)
        {
            return new TomlValue<T>(value);
        }

        internal static bool CanCreateFrom(Type t)
        {
            return t == StringType || t == TimespanType || IsFloatType(t) || IsIntegerType(t);
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

        private static bool IsFloatType(Type t)
        {
            return t == DoubleType || t == FloatType;
        }
    }

    [DebuggerDisplay("{value}:{typeof(T).FullName}")]
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
            var userConverter = config.GetFromTomlConverter(t);
            return userConverter != null ? userConverter.Convert(this.Value) : Converter.Convert(t, this.value);
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
