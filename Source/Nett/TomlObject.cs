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

        private IMetaDataStore metaData;
        internal IMetaDataStore MetaData => this.metaData;
        internal void SetAsMetaDataRoot(TomlTable.RootTable root) => this.metaData = root;

        public abstract string ReadableTypeName { get; }

        internal List<TomlComment> Comments { get; private set; }
        public T Get<T>() => (T)this.Get(typeof(T));
        public abstract object Get(Type t);

        internal TomlObject(IMetaDataStore metaData)
        {
            if (metaData == null && this.GetType() != typeof(TomlTable.RootTable)) { throw new ArgumentNullException(nameof(metaData)); }

            this.metaData = metaData;
            this.Comments = new List<TomlComment>();
        }

        internal static TomlObject CreateFrom(IMetaDataStore metaData, object val, PropertyInfo pi)
        {
            var t = val.GetType();
            var converter = metaData.Config.TryGetToTomlConverter(t);

            if (converter != null)
            {
                return (TomlObject)converter.Convert(metaData, val, Types.TomlObjectType);
            }
            else if (val as IDictionary != null)
            {
                return TomlTable.CreateFromDictionary(metaData, (IDictionary)val, metaData.Config.GetTableType(pi));
            }
            else if (t != StringType && (val as IEnumerable) != null)
            {
                return CreateArrayType(metaData, (IEnumerable)val);
            }
            else if (TomlValue.CanCreateFrom(t))
            {
                return TomlValue.ValueFrom(metaData, val);
            }
            else
            {
                var tableType = metaData.Config.GetTableType(pi);
                return TomlTable.CreateFromClass(metaData, val, tableType);
            }
        }

        private static TomlObject CreateArrayType(IMetaDataStore metaData, IEnumerable e)
        {
            var et = e.GetElementType();

            if (et != null)
            {
                var conv = metaData.Config.TryGetToTomlConverter(et);
                if (conv != null)
                {
                    if (conv.CanConvertTo(typeof(TomlValue)))
                    {
                        var values = e.Select((o) => (TomlValue)conv.Convert(metaData, o, Types.TomlValueType));
                        return new TomlArray(metaData, values.ToArray());
                    }
                    else if (conv.CanConvertTo(typeof(TomlTable)))
                    {
                        return new TomlTableArray(metaData, e.Select((o) => (TomlTable)conv.Convert(metaData, o, Types.TomlTableType)));
                    }
                    else
                    {
                        throw new NotSupportedException($"Cannot create array type from enumerable with element type {et.FullName}");
                    }
                }
                else if (TomlValue.CanCreateFrom(et))
                {
                    var values = e.Select((o) => TomlValue.ValueFrom(metaData, o));
                    return new TomlArray(metaData, values.ToArray());
                }
                else
                {
                    return new TomlTableArray(metaData, e.Select((o) => TomlTable.CreateFromClass(metaData, o)));
                }
            }
            else
            {
                var values = e.Select((o) => TomlValue.ValueFrom(metaData, o));
                return new TomlArray(metaData, values.ToArray());
            }
        }

        public abstract void Visit(ITomlObjectVisitor visitor);

        internal virtual void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            if (src.Comments.Count > 0 || overwriteWithEmpty)
            {
                this.Comments = new List<TomlComment>(src.Comments);
            }
        }
    }
}
