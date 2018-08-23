using System;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    [Serializable]
    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ParseException(
                  System.Runtime.Serialization.SerializationInfo info,
                  System.Runtime.Serialization.StreamingContext context)
                    : base(info, context)
        {
        }

        internal static ParseException FromSyntaxError(SyntaxErrorNode node)
            => Create(node.Location, node.Message);

        internal static ParseException MessageForNode(Node node, string message)
            => Create(node.Location, message);

        internal static ParseException TokenError(Token token, string message)
            => Create(token.Location, message);

        private static ParseException Create(SourceLocation location, string message)
            => new ParseException($"{location}: {message}");
    }
}
