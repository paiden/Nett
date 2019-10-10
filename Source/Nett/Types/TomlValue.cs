namespace Nett
{
    using System;
    using System.Collections.Generic;
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

        protected TomlValue(ITomlRoot root, object untyped)
            : base(root)
        {
            this.UntypedValue = untyped;
        }

        public object UntypedValue { get; }

        internal virtual string Unit { get; set; } = null;

        internal override object GetInternal(Type t, Func<IEnumerable<string>> _)
        {
            if (this.GetType() == t) { return this; }

            var converter = this.Root.Settings.TryGetConverter(this.GetType(), t);
            if (converter != null)
            {
                return converter.Convert(this.Root, this, t);
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert from type '{this.ReadableTypeName}' to '{t.Name}'.");
            }
        }

        protected static string CleanupNumberInputString(string input, int sub = 0)
            => input.Substring(sub).Replace("_", string.Empty).ToLowerInvariant();

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

        protected TomlValue(ITomlRoot root, T value)
            : base(root, value)
        {
            this.value = value;
        }

        public T Value => this.value;
    }
}
