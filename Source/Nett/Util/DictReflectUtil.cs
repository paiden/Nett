using System;
using System.Collections.Generic;
using System.Linq;

namespace Nett.Util
{
    internal static class DictReflectUtil
    {
        private static readonly Dictionary<Type, Entry> Calculated = new Dictionary<Type, Entry>();

        public static Type GetElementType(Type t)
        {
            if (Calculated.TryGetValue(t, out var e))
            {
                return e.ElementType;
            }
            else
            {
                var newEntry = Calc(t);
                Calculated[t] = newEntry;
                return newEntry.ElementType;
            }
        }

        private static Entry Calc(Type input)
        {
            return new Entry()
            {
                ElementType = GetFromInput() ?? GetFromInterfaces() ?? typeof(object),
            };

            Type GetFromInput()
                => input.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(input.GetGenericTypeDefinition())
                ? input.GetGenericArguments()[1]
                : null;

            Type GetFromInterfaces()
            {
                var interfaces = input.GetInterfaces();
                var genericIDictType = interfaces.FirstOrDefault(
                    i => i.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(i.GetGenericTypeDefinition()));

                return genericIDictType != null
                    ? genericIDictType.GetGenericArguments()[1]
                    : null;
            }
        }

        private struct Entry
        {
            public Type ElementType;
        }
    }
}
