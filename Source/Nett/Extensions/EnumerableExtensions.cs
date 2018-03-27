namespace Nett.LinqExtensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal static class EnumerableExtensions
    {
        public static Type GetElementType(this IEnumerable enumerable)
        {
            var et = enumerable.GetType();
            if (et.IsGenericType)
            {
                return et.GetGenericArguments()[0];
            }
            else
            {
                foreach (var e in enumerable)
                {
                    return e.GetType();
                }
            }

            return null;
        }

        public static IEnumerable<T> Select<T>(this IEnumerable enumerable, Func<object, T> selector)
        {
            foreach (var e in enumerable)
            {
                yield return selector(e);
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<object> ToGenEnumerable(this IEnumerable e)
            => e.Select(o => o);
    }
}
