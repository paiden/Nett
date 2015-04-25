using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    internal static class EnumerableExtensions
    {
        public static Type GetElementType(this IEnumerable enumerable)
        {
            var et = enumerable.GetType();
            if(et.IsGenericType)
            {
                return et.GetGenericArguments()[0];
            }
            else
            {
                foreach(var e in enumerable)
                {
                    return e.GetType();
                }
            }

            return null;
        }
    }
}
