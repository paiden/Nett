using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nett
{
    public abstract class TomlObject
    {
        private static readonly Type StringType = typeof(string);
        private static readonly Type EnumerableType = typeof(IEnumerable);

        public abstract string ReadableTypeName { get; }

        internal List<TomlComment> Comments { get; private set; }
        public T Get<T>() => (T)this.Get(typeof(T), TomlConfig.DefaultInstance);
        public T Get<T>(TomlConfig config) => (T)this.Get(typeof(T), config);
        public object Get(Type t) => this.Get(t, TomlConfig.DefaultInstance);
        public abstract object Get(Type t, TomlConfig config);

        internal static TomlObject From(object val, PropertyInfo pi, TomlConfig config)
        {
            var t = val.GetType();
            var converter = config.TryGetToTomlConverter(t);
            IEnumerable enumerable;
            if (converter != null)
            {
                return (TomlObject)converter.Convert(val, Types.TomlObjectType);
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
                var tableType = config.GetTableType(pi);
                return TomlTable.From(val, config, tableType);
            }
        }

        private static TomlObject CreateArrayType(IEnumerable e, TomlConfig config)
        {
            var et = e.GetElementType();

            if (et != null)
            {
                var conv = config.TryGetToTomlConverter(et);
                if (conv != null)
                {
                    if (conv.CanConvertTo(typeof(TomlValue)))
                    {
                        var values = e.Select((o) => (TomlValue)conv.Convert(o, Types.TomlValueType));
                        return new TomlArray(values.ToArray());
                    }
                    else if (conv.CanConvertTo(typeof(TomlTable)))
                    {
                        return new TomlTableArray(e.Select((o) => (TomlTable)conv.Convert(o, Types.TomlTableType)));
                    }
                    else
                    {
                        throw new NotSupportedException($"Cannot create array type from enumerable with element type {et.FullName}");
                    }
                }
                else if (TomlValue.CanCreateFrom(et))
                {
                    var values = e.Select((o) => TomlValue.ValueFrom(o));
                    return new TomlArray(values.ToArray());
                }
                else
                {
                    return new TomlTableArray(e.Select((o) => TomlTable.From(o, config)));
                }
            }
            else
            {
                var values = e.Select((o) => TomlValue.ValueFrom(o));
                return new TomlArray(values.ToArray());
            }
        }

        public abstract void Visit(ITomlObjectVisitor visitor);

        public TomlObject()
        {
            this.Comments = new List<TomlComment>();
        }

        internal virtual void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            if (src.Comments.Count > 0 || overwriteWithEmpty)
            {
                this.Comments = new List<TomlComment>(src.Comments);
            }
        }
    }
}
