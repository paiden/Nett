using System;
using System.Diagnostics;

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
        protected static readonly Type DateTimeType = typeof(DateTime);
        protected static readonly Type BoolType = typeof(bool);

        private static readonly Type[] IntTypes = new Type[]
            {
                CharType,
                Int16Type, Int32Type, Int64Type,
                UInt16Type, UInt32Type,
            };

        public static TomlValue ValueFrom(object val)
        {
            var targetType = val.GetType();

            if (StringType == targetType)
            {
                return new TomlString((string)val);
            }
            else if (TimespanType == targetType)
            {
                return new TomlTimeSpan((TimeSpan)val);
            }
            else if (IsFloatType(targetType))
            {
                return new TomlFloat((double)Convert.ChangeType(val, DoubleType));
            }
            else if (IsIntegerType(targetType))
            {
                return new TomlInt((long)Convert.ChangeType(val, Int64Type));
            }
            else if (DateTimeType == targetType)
            {
                return new TomlDateTime((DateTime)val);
            }
            else if (BoolType == targetType)
            {
                return new TomlBool((bool)val);
            }

            throw new NotSupportedException(string.Format("Cannot create TOML value from '{0}'.", targetType.FullName));
        }

        internal static bool CanCreateFrom(Type t) =>
            t == StringType || t == TimespanType || IsFloatType(t) || IsIntegerType(t) || t == DateTimeType || t == BoolType;

        private static bool IsIntegerType(Type t)
        {
            for (int i = 0; i < IntTypes.Length; i++)
            {
                if (IntTypes[i] == t)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFloatType(Type t) => t == DoubleType || t == FloatType;
    }

    [DebuggerDisplay("{value}:{typeof(T).FullName}")]
    public abstract class TomlValue<T> : TomlValue
    {
        private static readonly Type ValueType = typeof(T);
        private readonly T value;
        public T Value => this.value;

        public TomlValue(T value)
        {
            this.value = value;
        }

        public override object Get(Type t, TomlConfig config)
        {
            if (this.GetType() == t) { return this; }

            var userConverter = config.GetFromTomlConverter(t);
            return userConverter != null ? userConverter.Convert(this) : Converter.Convert(t, this.value);
        }
    }
}
