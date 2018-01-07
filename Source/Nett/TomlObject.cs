namespace Nett
{
    using System;
    using System.Collections.Generic;

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

        public abstract object Get(Type t);

        public abstract void Visit(ITomlObjectVisitor visitor);

        internal abstract TomlObject CloneFor(ITomlRoot root);

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
