using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal interface IHasAppendComment
    {
        Comment AppComment { get; }
    }

    internal interface IHasPrependComments
    {
        IEnumerable<Comment> PreComments { get; }
    }

    internal interface IHasComments : IHasAppendComment, IHasPrependComments
    {
    }

    internal sealed class Comment
    {
        public static readonly IEnumerable<Comment> NoComments = new List<Comment>();

        public static readonly Comment NoComment = null;

        public Comment(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get;
        }
    }
}
