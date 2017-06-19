using System.Diagnostics;

namespace Nett
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    internal class TomlComment
    {
        public TomlComment(string commentText, CommentLocation location)
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
    }
}
