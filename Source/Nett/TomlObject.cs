namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using LinqExtensions;

    [Flags]
    public enum TomlObjectType
    {
        Bool,
        Int,
        Float,
        String,
        DateTime,
        TimeSpan,
        Array,
        Table,
        ArrayOfTables,
    }

    public interface ITomlObject
    {
        string ReadableTypeName { get; }

        TomlObjectType TomlType { get; }
    }

    public abstract class TomlObject : ITomlObject
    {
        private static readonly Type EnumerableType = typeof(IEnumerable);

        private static readonly Type StringType = typeof(string);

        private IMetaDataStore metaData;

        internal TomlObject(IMetaDataStore metaData)
        {
            if (metaData == null && this.GetType() != typeof(TomlTable.RootTable)) { throw new ArgumentNullException(nameof(metaData)); }

            this.metaData = metaData;
            this.Comments = new List<TomlComment>();
        }

        public abstract string ReadableTypeName { get; }

        public abstract TomlObjectType TomlType { get; }

        internal List<TomlComment> Comments { get; private set; }

        internal IMetaDataStore MetaData => this.metaData;

        public T Get<T>() => (T)this.Get(typeof(T));

        public abstract object Get(Type t);

        public abstract void Visit(ITomlObjectVisitor visitor);

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
            else
            {
                var tableType = metaData.Config.GetTableType(pi);
                return TomlTable.CreateFromClass(metaData, val, tableType);
            }
        }

        internal virtual void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            if (src.Comments.Count > 0 || overwriteWithEmpty)
            {
                this.Comments = new List<TomlComment>(src.Comments);
            }
        }

        internal void SetAsMetaDataRoot(TomlTable.RootTable root) => this.metaData = root;

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
                else
                {
                    return new TomlTableArray(metaData, e.Select((o) => TomlTable.CreateFromClass(metaData, o)));
                }
            }

            return new TomlArray(metaData);
        }
    }
}
