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

        internal List<TomlComment> Comments { get; }
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
            IEnumerable enumerable;
            if (converter != null)
            {
                return (TomlObject)converter.Convert(val);
            }
            else if (t != StringType && (enumerable = val as IEnumerable) != null)
            {
                return CreateArrayType(enumerable, config);
            }
            else if (TomlValue.CanCreateFrom(t))
            {
                return TomlValue.ValueFrom(val);
            }
            else
            {
                return TomlTable.From(val, config);
            }
        }

        private static TomlObject CreateArrayType(IEnumerable e, TomlConfig config)
        {
            var et = e.GetElementType();

            if (et != null)
            {
                var conv = config.GetToTomlConverter(et);
                if (conv != null)
                {
                    if (typeof(TomlValue).IsAssignableFrom(conv.ToType))
                    {
                        var values = e.Select((o) => (TomlValue)conv.Convert(o));
                        return new TomlArray(values);
                    }
                    else if (typeof(TomlTable).IsAssignableFrom(conv.ToType))
                    {
                        return new TomlTableArray("", e.Select((o) => (TomlTable)conv.Convert(o)), config);
                    }
                    else
                    {
                        throw new NotSupportedException($"Cannot create array type from enumerable with elemen type {et.FullName}");
                    }
                }
                else if (TomlValue.CanCreateFrom(et))
                {
                    var values = e.Select((o) => TomlValue.ValueFrom(o));
                    return new TomlArray(values);
                }
                else
                {
                    return new TomlTableArray("", e.Select((o) => TomlTable.From(o, config)), config);
                }
            }
            else
            {
                var values = e.Select((o) => TomlValue.ValueFrom(o));
                return new TomlArray(values);
            }
        }

        public virtual bool IsTable => false;

        public virtual bool IsTableArray => false;

        public TomlObject()
        {
            this.Comments = new List<TomlComment>();
        }
    }
}
