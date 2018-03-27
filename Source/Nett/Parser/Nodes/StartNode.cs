using System.Collections.Generic;
using Nett.Parser.Builders;

namespace Nett.Parser.Nodes
{
    internal sealed class StartNode : NextExpressionNode
    {
        public StartNode(IReq<ExpressionNode> expression, IOpt<NextExpressionNode> next)
            : base(expression, next)
        {
        }

        public override string ToString()
            => "S";

        public IEnumerable<ExpressionNode> Expressions()
        {
            return LinearizeExpressions(this);
        }

        public TomlTable CreateTable()
        {
            var table = TableBuilder.Build(this, TomlSettings.DefaultInstance);
            return table;
        }
    }
}
