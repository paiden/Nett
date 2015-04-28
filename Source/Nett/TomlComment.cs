using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    internal class TomlComment
    {
        public string CommentText { get; }
        public CommentLocation Location { get; }

        public TomlComment(string commentText, CommentLocation location)
        {
            this.CommentText = commentText;
            this.Location = location;
        }
    }
}
