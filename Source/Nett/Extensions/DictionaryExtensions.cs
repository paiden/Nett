namespace Nett.Extensions
{
    using System;
    using System.Collections.Generic;

    internal static class DictionaryExtensions
    {
        public static TValue AddIfNeeded<TKey, TValue>(this IDictionary<TKey, TValue> target, TKey key, TValue def)
            => AddIfNeeded(target, key, () => def);

        public static TValue AddIfNeeded<TKey, TValue>(this IDictionary<TKey, TValue> target, TKey key, Func<TValue> initializer)
        {
            if (target == null) { throw new ArgumentNullException(nameof(target)); }
            if (key == null) { throw new ArgumentNullException(nameof(key)); }

            TValue tgtVal;

            if (!target.TryGetValue(key, out tgtVal))
            {
                var val = initializer();
                target[key] = val;
                return val;
            }

            return tgtVal;
        }
    }
}
