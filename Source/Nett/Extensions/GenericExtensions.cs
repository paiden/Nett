using System;
using System.Collections.Generic;

namespace Nett.Extensions
{
    internal static class GenericExtensions
    {
        public static T CheckNotNull<T>(this T toCheck, string argName)
            where T : class
        {
            if (toCheck == null) { throw new ArgumentNullException(argName); }

            return toCheck;
        }

        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }
    }
}
