using System.Linq.Expressions;
using Nett.Coma.Path;
using Nett.Extensions;

namespace Nett.Coma.Extensions
{
    internal static class LambdaExpressionExtensions
    {
        public static TPath BuildTPath(this LambdaExpression expr)
        {
            expr.CheckNotNull(nameof(expr));
            return TPath.Build(expr);
        }
    }
}
