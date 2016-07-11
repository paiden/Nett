namespace Nett
{
    using System;

    public enum CommentLocation
    {
        UseDefault,
        Prepend,
        Append,
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class TomlCommentAttribute : Attribute
    {
        public TomlCommentAttribute(string comment, CommentLocation location = CommentLocation.UseDefault)
        {
            this.Comment = comment;
            this.Location = location;
        }

        public string Comment { get; }

        public CommentLocation Location { get; }
    }
}
