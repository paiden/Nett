using System.Linq.Expressions;
using Nett.Extensions;

namespace Nett.Coma.Extensions
{
    internal static class LambdaExpressionExtensions
    {
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
