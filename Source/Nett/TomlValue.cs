namespace Nett
{
    using System;
    using System.Diagnostics;

    public abstract class TomlValue : TomlObject
    {
        protected static readonly Type BoolType = typeof(bool);
        protected static readonly Type CharType = typeof(char);
        protected static readonly Type DateTimeType = typeof(DateTime);
        protected static readonly Type DoubleType = typeof(double);
        protected static readonly Type FloatType = typeof(float);
        protected static readonly Type Int16Type = typeof(short);
        protected static readonly Type Int32Type = typeof(int);
        protected static readonly Type Int64Type = typeof(long);
        protected static readonly Type StringType = typeof(string);
        protected static readonly Type TimespanType = typeof(TimeSpan);
        protected static readonly Type UInt16Type = typeof(ushort);
        protected static readonly Type UInt32Type = typeof(uint);

        private static readonly Type[] IntTypes = new Type[]
            {
                CharType,
                Int16Type, Int32Type, Int64Type,
                UInt16Type, UInt32Type,
            };

        public TomlValue(IMetaDataStore metaData)
            : base(metaData)
        {
        }

        public static TomlValue ValueFrom(IMetaDataStore metaData, object val)
        {
            var targetType = val.GetType();

            if (targetType == StringType)
            {
                return new TomlString(metaData, (string)val);
            }
            else if (targetType == TimespanType)
            {
                return new TomlTimeSpan(metaData, (TimeSpan)val);
            }
            else if (IsFloatType(targetType))
            {
                return new TomlFloat(metaData, (double)Convert.ChangeType(val, DoubleType));
            }
            else if (IsIntegerType(targetType))
            {
                return new TomlInt(metaData, (long)Convert.ChangeType(val, Int64Type));
            }
            else if (targetType == DateTimeType)
            {
                return new TomlDateTime(metaData, (DateTime)val);
            }
            else if (targetType == BoolType)
            {
                return new TomlBool(metaData, (bool)val);
            }

            throw new NotSupportedException(string.Format("Cannot create TOML value from '{0}'.", targetType.FullName));
        }

        internal static bool CanCreateFrom(Type t) =>
            t == StringType || t == TimespanType || IsFloatType(t) || IsIntegerType(t) || t == DateTimeType || t == BoolType;

        private static bool IsFloatType(Type t) => t == DoubleType || t == FloatType;

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
    }

    [DebuggerDisplay("{value}:{typeof(T).FullName}")]
    public abstract class TomlValue<T> : TomlValue
    {
        private static readonly Type ValueType = typeof(T);
        private readonly T value;

        public TomlValue(IMetaDataStore metaData, T value)
            : base(metaData)
        {
            this.value = value;
        }

        public T Value => this.value;

        public override object Get(Type t)
        {
            if (this.GetType() == t) { return this; }

            var converter = this.MetaData.Config.TryGetConverter(this.GetType(), t);
            if (converter != null)
            {
                return converter.Convert(this.MetaData, this, t);
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert from type '{this.ReadableTypeName}' to '{t.Name}'.");
            }
        }
    }
}
