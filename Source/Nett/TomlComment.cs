namespace Nett
{
    internal class TomlComment
    {
        public TomlComment(string commentText, CommentLocation location)
        {
            this.Text = commentText;
            this.Location = location;
        }

        public CommentLocation Location { get; }

        public string Text { get; }
    }
}
