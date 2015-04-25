using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    enum TomlObjectType
    {
        Array,
        Value,
    }

    public abstract class TomlObject
    {
        private static readonly Type StringType = typeof(string);
        private static readonly Type EnumerableType = typeof(IEnumerable);

        internal List<TomlComment> Comments { get; private set; }
        public abstract T Get<T>();
        public abstract T Get<T>(TomlConfig config);
        public abstract object Get(Type t);
        public abstract object Get(Type t, TomlConfig config);
        public abstract void WriteTo(StreamWriter writer);
        public abstract void WriteTo(StreamWriter writer, TomlConfig config);

        internal static TomlObject From(object val, TomlConfig config)
        {
            var t = val.GetType();
            var converter = config.GetToTomlConverter(t);
            if (converter != null)
            {
                return (TomlObject)converter.Convert(val);
            }
            else if (t != StringType && EnumerableType.IsAssignableFrom(t))
            {
                var e = (IEnumerable)val;
                var et = e.GetElementType();
                if (et != null && !TomlValue.CanCreateFrom(et))
                {
                    return new TomlTableArray("", e, config);
                }
                else
                {
                    return new TomlArray((IEnumerable)val, config);
                }
            }
            else if (TomlValue.CanCreateFrom(t))
            {
                return TomlValue.From(val, t);
            }
            else
            {
                return TomlTable.From(val, config);
            }
        }

        public bool IsTable
        {
            get
            {
                return this.GetType() == Types.TomlTableType;
            }
        }

        public bool IsTableArray
        {
            get
            {
                var a = this as TomlArray;
                return a != null && a.IsTableArrayInternal();
            }
        }

        public TomlObject()
        {
            this.Comments = new List<TomlComment>();
        }
    }
}
