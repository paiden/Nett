using System;
using System.Collections.Generic;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        // Values
        public static TomlBool Update(this TomlTable table, string key, bool value)
            => Update(table, key, table.CreateAttached(value));

        public static TomlString Update(this TomlTable table, string key, string value)
            => Update(table, key, table.CreateAttached(value));

        public static TomlInt Update(this TomlTable table, string key, long value)
            => Update(table, key, table.CreateAttached(value));

        public static TomlFloat Update(this TomlTable table, string key, double value)
            => Update(table, key, table.CreateAttached(value));

        public static TomlDateTime Update(this TomlTable table, string key, DateTimeOffset value)
            => Update(table, key, table.CreateAttached(value));

        public static TomlTimeSpan Update(this TomlTable table, string key, TimeSpan value)
            => Update(table, key, table.CreateAttached(value));

        public static TomlTable Update(
            this TomlTable table, string key, object obj, TomlTable.TableTypes type = TomlTable.TableTypes.Default)
            => Update(table, key, TomlTable.CreateFromClass(table.Root, obj, type));

        // Arrays
        public static TomlArray Update(this TomlTable table, string key, IEnumerable<bool> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<string> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<long> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<int> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<double> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<float> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<DateTimeOffset> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<DateTime> array)
            => Update(table, key, table.CreateAttached(array));

        public static TomlArray Update(this TomlTable table, string key, IEnumerable<TimeSpan> array)
            => Update(table, key, table.CreateAttached(array));

        private static T Update<T>(TomlTable table, string key, T obj)
            where T : TomlObject
        {
            if (table.TryGetValue(key, out var _))
            {
                table.InternalRows[new TomlKey(key)] = obj;
                return obj;
            }
            else
            {
                throw new InvalidOperationException($"Cannot update because row with key '{key}' does not exist.");
            }
        }
    }
}
