namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using LinqExtensions;
    using Nett.Parser.Nodes;

    public enum TomlObjectType
    {
        Bool = 1,
        Int,
        Float,
        String,
        DateTime,
        LocalDateTime,
        LocalTime,
        LocalDate,
        TimeSpan,
        Array,
        Table,
        ArrayOfTables,
    }

    public abstract class TomlObject
    {
        private List<TomlComment> comments;

        internal TomlObject(ITomlRoot root)
        {
            if (root == null && this.GetType() != typeof(TomlTable.RootTable)) { throw new ArgumentNullException(nameof(root)); }

            this.Root = root ?? (TomlTable.RootTable)this;
            this.comments = new List<TomlComment>();
        }

        public IEnumerable<TomlComment> Comments => this.comments;

        public abstract string ReadableTypeName { get; }

        public abstract TomlObjectType TomlType { get; }

        internal ITomlRoot Root { get; }

        public TomlObject AddComment(TomlComment comment)
        {
            this.comments.Add(comment);
            return this;
        }

        public TomlObject AddComment(string text, CommentLocation locaction = CommentLocation.UseDefault)
            => this.AddComment(new TomlComment(text, locaction));

        public TomlObject AddComments(IEnumerable<TomlComment> comments)
        {
            this.comments.AddRange(comments);
            return this;
        }

        public void ClearComments() => this.comments.Clear();

        public T Get<T>() => (T)this.Get(typeof(T));

        public object Get(Type t)
            => this.GetInternal(t, getMyKeyChain: Enumerable.Empty<string>);

        public abstract void Visit(ITomlObjectVisitor visitor);

        internal static TomlObject CreateFrom(ITomlRoot root, object val)
            => CreateFrom(root, val, null, val.GetType());

        internal TomlObject AddComments(IHasComments n)
        {
            this.AddPreComments(n);
            this.AddAppComment(n);
            return this;
        }

        internal TomlObject AddPreComments(IHasPrependComments source)
        {
            foreach (var c in source.PreComments)
            {
                this.AddComment(c.Value, CommentLocation.Prepend);
            }

            return this;
        }

        internal TomlObject AddAppComment(IHasAppendComment source)
        {
            if (source.AppComment != null)
            {
                this.AddComment(source.AppComment.Value, CommentLocation.Append);
            }

            return this;
        }

        internal abstract TomlObject CloneFor(ITomlRoot root);

        internal abstract object GetInternal(Type t, Func<IEnumerable<string>> getMyKeyChain);

        internal virtual void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            if (src.comments.Count > 0 || overwriteWithEmpty)
            {
                this.comments = new List<TomlComment>(src.comments);
            }
        }

        internal abstract TomlObject WithRoot(ITomlRoot root);

        protected static T CopyComments<T>(T dst, TomlObject src)
            where T : TomlObject
        {
            dst.comments.AddRange(src.comments);
            return dst;
        }

        private static TomlObject CreateArrayType(ITomlRoot root, IEnumerable e)
        {
            if (!e.ToGenEnumerable().Any())
            {
                var et = e.GetElementType();
                if (et == null) { return new TomlArray(root); }

                var conv = root.Settings.TryGetToTomlConverter(et);
                return conv != null && conv.CanConvertTo(typeof(TomlValue))
                    ? (TomlObject)new TomlArray(root)
                    : new TomlTableArray(root);
            }

            var items = e.Select(o => CreateFrom(root, o, pi: null, valueType: e.GetElementType())).ToList();
            var types = items.Select(i => i.GetType()).Distinct();
            if (types.Count() > 1)
            {
                throw new ArgumentException($"Enumerable mapped to multiple TOML types '{string.Join(", ", types)}'. " +
                    $"All array elements must map to the same TOML type.");
            }

            if (items.Any() && items.First().TomlType == TomlObjectType.Table)
            {
                return new TomlTableArray(root, items.Cast<TomlTable>());
            }

            return new TomlArray(root, items.Cast<TomlValue>().ToArray());
        }

        private static TomlObject CreateFrom(ITomlRoot root, object val, PropertyInfo pi, Type valueType)
        {
            var t = valueType;
            var converter = root.Settings.TryGetToTomlConverter(t);

            if (converter != null)
            {
                return (TomlObject)converter.Convert(root, val, Types.TomlObjectType);
            }
            else if (val as IDictionary != null)
            {
                return TomlTable.CreateFromDictionary(root, (IDictionary)val, root.Settings.GetTableType(t));
            }
            else if (t != Types.StringType && (val as IEnumerable) != null)
            {
                return CreateArrayType(root, (IEnumerable)val);
            }
            else
            {
                var tableType = root.Settings.GetTableType(t);
                return TomlTable.CreateFromComplexObject(root, val, tableType);
            }
        }
    }

    internal static class TomlObjecTypeExtensions
    {
        public static Type ToTomlClassType(this TomlObjectType t)
        {
            switch (t)
            {
                case TomlObjectType.Bool: return Types.TomlBoolType;
                case TomlObjectType.String: return Types.TomlStringType;
                case TomlObjectType.Int: return Types.TomlIntType;
                case TomlObjectType.Float: return Types.TomlFloatType;
                case TomlObjectType.DateTime: return Types.TomlDateTimeType;
                case TomlObjectType.LocalDate: return Types.TomlLocalDate;
                case TomlObjectType.LocalDateTime: return Types.TomlLocalDateTime;
                case TomlObjectType.LocalTime: return Types.TomlLocalTime;
                case TomlObjectType.TimeSpan: return Types.TomlTimeSpanType;
                case TomlObjectType.Array: return Types.TomlArrayType;
                case TomlObjectType.Table: return Types.TomlTableType;
                case TomlObjectType.ArrayOfTables: return Types.TomlTableArrayType;
                default:
                    throw new InvalidOperationException($"Cannot convert '{t}' to corresponding class type.");
            }
        }
    }
}
