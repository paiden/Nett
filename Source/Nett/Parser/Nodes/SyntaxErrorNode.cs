using System.Collections.Generic;
using System.Linq;

namespace Nett.Parser.Nodes
{
    internal sealed class SyntaxErrorNode : Node
    {
        private readonly SourceLocation location;

        public SyntaxErrorNode(string error, SourceLocation location)
        {
            this.Message = error;
            this.location = location;
        }

        public override SourceLocation Location
            => this.location;

        public string Message { get; }

        public override IEnumerable<Node> Children
            => Enumerable.Empty<Node>();

        public static SyntaxErrorNode Unexpected(Token unexpected)
        {
            return unexpected.TokenError()
                ?? new SyntaxErrorNode($"Encountered {UnexpectedTkn(unexpected)}.", unexpected.Location);
        }

        public static SyntaxErrorNode Unexpected(string message, Token current)
        {
            return current.TokenError()
                ?? new SyntaxErrorNode($"{message} but encountered {UnexpectedTkn(current)}.", current.Location);
        }

        public override string ToString()
            => "X";

        private static string UnexpectedTkn(Token tkn)
            => $"unexpected token of type '{tkn.Type}' with value '{tkn.Value}'";
    }
}
