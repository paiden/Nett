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

    public abstract class TomlObject
    {
        internal TomlObject(ITomlRoot root)
        {
            if (root == null && this.GetType() != typeof(TomlTable.RootTable)) { throw new ArgumentNullException(nameof(root)); }

            this.Root = root ?? (TomlTable.RootTable)this;
            this.Comments = new List<TomlComment>();
        }

        public abstract string ReadableTypeName { get; }

        public abstract TomlObjectType TomlType { get; }

        internal List<TomlComment> Comments { get; private set; }

        internal ITomlRoot Root { get; }

        public T Get<T>() => (T)this.Get(typeof(T));

        public abstract object Get(Type t);

        public abstract void Visit(ITomlObjectVisitor visitor);

        internal static TomlObject CreateFrom(ITomlRoot root, object val, PropertyInfo pi)
        {
            var t = val.GetType();
            var converter = root.Config.TryGetToTomlConverter(t);

            if (converter != null)
            {
                return (TomlObject)converter.Convert(root, val, Types.TomlObjectType);
            }
            else if (val as IDictionary != null)
            {
                return TomlTable.CreateFromDictionary(root, (IDictionary)val, root.Config.GetTableType(t, pi));
            }
            else if (t != Types.StringType && (val as IEnumerable) != null)
            {
                return CreateArrayType(root, (IEnumerable)val);
            }
            else
            {
                var tableType = root.Config.GetTableType(t, pi);
                return TomlTable.CreateFromClass(root, val, tableType);
            }
        }

        internal virtual void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            if (src.Comments.Count > 0 || overwriteWithEmpty)
            {
                this.Comments = new List<TomlComment>(src.Comments);
            }
        }

        internal abstract TomlObject WithRoot(ITomlRoot root);

        private static TomlObject CreateArrayType(ITomlRoot root, IEnumerable e)
        {
            var et = e.GetElementType();

            if (et != null)
            {
                var conv = root.Config.TryGetToTomlConverter(et);
                if (conv != null)
                {
                    if (conv.CanConvertTo(typeof(TomlValue)))
                    {
                        var values = e.Select((o) => (TomlValue)conv.Convert(root, o, Types.TomlValueType));
                        return new TomlArray(root, values.ToArray());
                    }
                    else if (conv.CanConvertTo(typeof(TomlTable)))
                    {
                        return new TomlTableArray(root, e.Select((o) => (TomlTable)conv.Convert(root, o, Types.TomlTableType)));
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"Cannot create array type from enumerable with element type {et.FullName}");
                    }
                }
                else
                {
                    return new TomlTableArray(root, e.Select((o) =>
                        TomlTable.CreateFromClass(root, o, root.Config.GetTableType(et, null))));
                }
            }

            return new TomlArray(root);
        }
    }
}
