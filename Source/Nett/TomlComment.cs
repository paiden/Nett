using System;
using System.Diagnostics;

namespace Nett
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public sealed class TomlComment : IEquatable<TomlComment>
    {
        public TomlComment(string commentText, CommentLocation location = CommentLocation.UseDefault)
        {
            this.Text = commentText;
            this.Location = location;
        }

        public CommentLocation Location { get; }

        public string Text { get; }

        private string DebuggerDisplay
        {
            get
            {
                var prefix = this.Location == CommentLocation.Prepend ? "P" : "A";
                return $"{prefix} #{this.Text}";
            }
        }

        public override bool Equals(object obj) => this.Equals(obj as TomlComment);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + this.Text.GetHashCode();
                hash = (hash * 23) + this.Location.GetHashCode();
                return hash;
            }
        }

        public bool Equals(TomlComment other) => other != null && other.Text == this.Text && other.Location == this.Location;
    }
}
