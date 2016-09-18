using System.Collections.Generic;
using System.Linq.Expressions;
using Nett.Extensions;

namespace Nett.Coma.Extensions
{
    internal static class LambdaExpressionExtensions
    {
        public static KeyChain ResolveKeyChain(this LambdaExpression expr)
        {
            expr.CheckNotNull(nameof(expr));

            List<string> keys = new List<string>();
            MemberExpression me;

            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expr.Body as UnaryExpression;
                    me = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            while (me != null)
            {
                keys.Insert(0, me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            return new KeyChain(keys);
        }

        public static TPath BuildTPath(this LambdaExpression expr)
        {
            expr.CheckNotNull(nameof(expr));

            var exprBody = expr.Body.ToString();
            var withoutRootKey = exprBody.Substring(exprBody.IndexOf(".") + 1);

            var toParse = "/" + withoutRootKey.Replace('.', '/');
            return TPath.Parse(toParse);
        }
    }
}
