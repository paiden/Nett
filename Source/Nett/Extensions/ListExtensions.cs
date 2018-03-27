using System.Collections.Generic;

namespace Nett.Extensions
{
    internal static class ListExtensions
    {
        public static T PopBack<T>(this IList<T> l)
        {
            var ele = l[l.Count - 1];
            l.RemoveAt(l.Count - 1);
            return ele;
        }
    }
}
