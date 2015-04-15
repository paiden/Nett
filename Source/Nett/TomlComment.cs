using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    internal class TomlComment
    {
        public string CommentText { get; private set; }
        public CommentLocation Location { get; private set; }

        public TomlComment(string commentText, CommentLocation location)
        {
            this.CommentText = commentText;
            this.Location = location;
        }
    }
}
