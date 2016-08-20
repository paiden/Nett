namespace Nett.Extensions
{
    using System;
    using System.Collections.Generic;

    internal static class DictionaryExtensions
    {
        public static TValue AddIfNeeded<TKey, TValue>(this IDictionary<TKey, TValue> target, TKey key, TValue def)
        {
            if (target == null) { throw new ArgumentNullException(nameof(target)); }
            if (key == null) { throw new ArgumentNullException(nameof(key)); }

            TValue tgtVal;

            if (!target.TryGetValue(key, out tgtVal))
            {
                target[key] = def;
                return def;
            }

            return tgtVal;
        }
    }
}
