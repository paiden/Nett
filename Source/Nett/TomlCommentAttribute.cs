using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public enum CommentLocation
    {
        UseDefault,
        Prepend,
        Append,
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class TomlCommentAttribute : Attribute
    {
        public string Comment { get; private set; }
        public CommentLocation Location { get; private set; }

        public TomlCommentAttribute(string comment, CommentLocation location = CommentLocation.UseDefault)
        {
            this.Comment = comment;
            this.Location = location;
        }
    }
}
